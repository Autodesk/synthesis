#!/usr/bin/env python3

"""Compare two .mira assembly exports for semantic equivalence.

Usage:
    python isotope/tools/compare_mira.py <reference.mira> <test.mira>

Designed to compare a Python-exporter file (reference) against a C++ Isotope
file (test).  The following known format differences are handled automatically:

  - info.version        : 5 (Python) vs 6+ (C++) - ignored
  - appearance map keys : "{name}_{id}" (Python) vs "{id}" (C++) - resolved
                          by info.name before comparison
  - signal map keys     : random UUID per-export - matched by info.name
  - thumbnail           : present in Python, absent in C++ - skipped
  - "default" entries   : hardcoded constants in both exporters - skipped

Exit code 0 if all sections pass, 1 if any section fails.
"""

from __future__ import annotations

import argparse
import gzip
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

_PROTO_DIR = (
    Path(__file__).parent.parent.parent
    / "exporter"
    / "SynthesisFusionAddin"
    / "src"
    / "Proto"
)
sys.path.insert(0, str(_PROTO_DIR))

import assembly_pb2  # type: ignore
import joint_pb2  # type: ignore
import material_pb2  # type: ignore
import signal_pb2  # type: ignore
import types_pb2  # type: ignore

FLOAT_TOL = 1e-4
ISSUE_CAP = 25  # max issues printed per section


@dataclass
class Issue:
    path: str
    ref_val: Any
    test_val: Any


@dataclass
class Section:
    name: str
    issues: list[Issue] = field(default_factory=list)
    checks: int = 0

    def ok(self) -> bool:
        return not self.issues

    def check(
        self,
        path: str,
        ref: Any,
        test: Any,
        tol: float | None = None,
    ) -> bool:
        self.checks += 1
        if tol is not None:
            passed = abs(float(ref) - float(test)) <= tol
        else:
            passed = ref == test
        if not passed:
            self.issues.append(Issue(path, ref, test))
        return passed

    def missing(self, label: str) -> None:
        self.checks += 1
        self.issues.append(Issue(label, "present", "absent"))

    def extra(self, label: str) -> None:
        self.checks += 1
        self.issues.append(Issue(label, "absent", "present"))

    def report(self) -> None:
        mark = "✓" if self.ok() else "✗"
        print(f"\n=== {self.name} {mark} ===")
        if self.ok():
            print(f"  {self.checks} check(s) passed")
            return
        passed = self.checks - len(self.issues)
        print(f"  {passed}/{self.checks} passed  ({len(self.issues)} issue(s))")
        shown = self.issues[:ISSUE_CAP]
        for iss in shown:
            print(f"  ✗  {iss.path}")
            print(f"       ref  = {iss.ref_val!r}")
            print(f"       test = {iss.test_val!r}")
        if len(self.issues) > ISSUE_CAP:
            print(f"  ... and {len(self.issues) - ISSUE_CAP} more issue(s) not shown")


def load(path: str) -> assembly_pb2.Assembly:
    data = Path(path).read_bytes()
    if data[:2] == b"\x1f\x8b":
        data = gzip.decompress(data)
    asm = assembly_pb2.Assembly()
    asm.ParseFromString(data)
    return asm


def diff_keys(
    s: Section,
    ref_keys: set[str],
    test_keys: set[str],
    label: str,
) -> None:
    s.check(f"{label} count", len(ref_keys), len(test_keys))
    for k in sorted(ref_keys - test_keys):
        s.missing(f"{label} key missing in test: '{k}'")
    for k in sorted(test_keys - ref_keys):
        s.extra(f"{label} extra key in test: '{k}'")


def flatten_nodes(node: types_pb2.Node) -> list[str]:
    """Return all node.value strings in breadth-first order."""
    out: list[str] = []
    queue = [node]
    while queue:
        n = queue.pop(0)
        if n.value:
            out.append(n.value)
        queue.extend(n.children)
    return out


def appearance_name(appearances: dict, key: str) -> str:
    """Resolve an appearance map key to info.name, or return the key itself."""
    entry = appearances.get(key)
    return entry.info.name if entry else key


def compare_part_definitions(
    ref_data: assembly_pb2.AssemblyData,
    test_data: assembly_pb2.AssemblyData,
) -> Section:
    s = Section("PART DEFINITIONS")
    ref_defs = dict(ref_data.parts.part_definitions)
    test_defs = dict(test_data.parts.part_definitions)
    ref_appearances = dict(ref_data.materials.appearances)
    test_appearances = dict(test_data.materials.appearances)

    diff_keys(s, set(ref_defs), set(test_defs), "part_definitions")

    for key in sorted(set(ref_defs) & set(test_defs)):
        rd = ref_defs[key]
        td = test_defs[key]

        s.check(f"{key}: body count", len(rd.bodies), len(td.bodies))

        for i, (rb, tb) in enumerate(zip(rd.bodies, td.bodies)):
            prefix = f"{key}[body {i}]"
            rm = rb.triangle_mesh.mesh
            tm = tb.triangle_mesh.mesh

            s.check(f"{prefix}: verts count", len(rm.verts), len(tm.verts))
            s.check(f"{prefix}: normals count", len(rm.normals), len(tm.normals))
            s.check(f"{prefix}: indices count", len(rm.indices), len(tm.indices))
            s.check(f"{prefix}: uv count", len(rm.uv), len(tm.uv))

            # Compare vertex sets order-independently: Fusion's tessellator may
            # return the same geometry with different vertex orderings between
            # sessions, so element-wise comparison would produce false failures.
            # Sorting the (x, y, z) tuples gives a session-stable geometric check.
            if len(rm.verts) == len(tm.verts) and len(rm.verts) % 3 == 0:
                rv_sorted = sorted(
                    (round(rm.verts[i], 4), round(rm.verts[i+1], 4), round(rm.verts[i+2], 4))
                    for i in range(0, len(rm.verts), 3)
                )
                tv_sorted = sorted(
                    (round(tm.verts[i], 4), round(tm.verts[i+1], 4), round(tm.verts[i+2], 4))
                    for i in range(0, len(tm.verts), 3)
                )
                for idx, (rv, tv) in enumerate(zip(rv_sorted, tv_sorted)):
                    if rv != tv:
                        s.issues.append(Issue(f"{prefix}: verts (sorted)[{idx}]", rv, tv))
                s.checks += 1

            # Appearance override - resolve through each exporter's own map by name
            ref_app = appearance_name(ref_appearances, rb.appearance_override)
            test_app = appearance_name(test_appearances, tb.appearance_override)
            s.check(f"{prefix}: appearance_override (resolved)", ref_app, test_app)

        # Physical properties
        rp = rd.physical_data
        tp = td.physical_data
        s.check(f"{key}: mass", rp.mass, tp.mass, tol=FLOAT_TOL)
        s.check(f"{key}: volume", rp.volume, tp.volume, tol=FLOAT_TOL)
        s.check(f"{key}: density", rp.density, tp.density, tol=FLOAT_TOL)

    return s


def compare_part_instances(
    ref_parts: assembly_pb2.Parts,
    test_parts: assembly_pb2.Parts,
) -> Section:
    s = Section("PART INSTANCES")
    ref_inst = dict(ref_parts.part_instances)
    test_inst = dict(test_parts.part_instances)

    diff_keys(s, set(ref_inst), set(test_inst), "part_instances")

    for key in sorted(set(ref_inst) & set(test_inst)):
        ri = ref_inst[key]
        ti = test_inst[key]

        s.check(f"{key}: part_definition_reference", ri.part_definition_reference, ti.part_definition_reference)
        s.check(f"{key}: skip_collider", ri.skip_collider, ti.skip_collider)

        rm = list(ri.global_transform.spatial_matrix)
        tm = list(ti.global_transform.spatial_matrix)
        s.check(f"{key}: global_transform length", len(rm), len(tm))
        for idx, (rv, tv) in enumerate(zip(rm, tm)):
            s.check(f"{key}: global_transform[{idx}]", rv, tv, tol=FLOAT_TOL)

    return s


def compare_joint_definitions(
    ref_joints: joint_pb2.Joints,
    test_joints: joint_pb2.Joints,
) -> Section:
    s = Section("JOINT DEFINITIONS")
    ref_defs = dict(ref_joints.joint_definitions)
    test_defs = dict(test_joints.joint_definitions)

    diff_keys(s, set(ref_defs), set(test_defs), "joint_definitions")

    for key in sorted(set(ref_defs) & set(test_defs)):
        rj = ref_defs[key]
        tj = test_defs[key]

        s.check(f"{key}: joint_motion_type", rj.joint_motion_type, tj.joint_motion_type)
        s.check(f"{key}: origin.x", rj.origin.x, tj.origin.x, tol=FLOAT_TOL)
        s.check(f"{key}: origin.y", rj.origin.y, tj.origin.y, tol=FLOAT_TOL)
        s.check(f"{key}: origin.z", rj.origin.z, tj.origin.z, tol=FLOAT_TOL)

        motion = rj.WhichOneof("JointMotion")
        if motion == "rotational":
            _compare_dof(s, key, "rotational", rj.rotational.rotational_freedom, tj.rotational.rotational_freedom)
        elif motion == "prismatic":
            _compare_dof(s, key, "prismatic", rj.prismatic.prismatic_freedom, tj.prismatic.prismatic_freedom)

    return s


def _compare_dof(s: Section, key: str, kind: str, rd, td) -> None:
    p = f"{key}: {kind}"
    s.check(f"{p}.axis.x", rd.axis.x, td.axis.x, tol=FLOAT_TOL)
    s.check(f"{p}.axis.y", rd.axis.y, td.axis.y, tol=FLOAT_TOL)
    s.check(f"{p}.axis.z", rd.axis.z, td.axis.z, tol=FLOAT_TOL)
    s.check(f"{p}.limits.lower", rd.limits.lower, td.limits.lower, tol=FLOAT_TOL)
    s.check(f"{p}.limits.upper", rd.limits.upper, td.limits.upper, tol=FLOAT_TOL)
    s.check(f"{p}.limits.velocity", rd.limits.velocity, td.limits.velocity, tol=FLOAT_TOL)


def compare_joint_instances(
    ref_joints: joint_pb2.Joints,
    test_joints: joint_pb2.Joints,
) -> Section:
    s = Section("JOINT INSTANCES")
    ref_inst = dict(ref_joints.joint_instances)
    test_inst = dict(test_joints.joint_instances)

    diff_keys(s, set(ref_inst), set(test_inst), "joint_instances")

    # Build a set of valid joint_definition GUIDs within each export for
    # reference-validity checks below.
    ref_def_guids  = {v.info.GUID for v in ref_joints.joint_definitions.values()}
    test_def_guids = {v.info.GUID for v in test_joints.joint_definitions.values()}

    for key in sorted(set(ref_inst) & set(test_inst)):
        ri = ref_inst[key]
        ti = test_inst[key]

        if key == "grounded":
            # The grounded joint definition GUID is generated differently between
            # exporters (random uuid4 in C++, name-derived in Python).  Instead of
            # comparing the UUID literals, verify that each export's reference
            # resolves to a definition that exists within that same export.
            if ri.joint_reference not in ref_def_guids:
                s.missing(f"grounded: ref joint_reference not found in ref definitions")
            if ti.joint_reference not in test_def_guids:
                s.missing(f"grounded: test joint_reference not found in test definitions")
        else:
            s.check(f"{key}: joint_reference", ri.joint_reference, ti.joint_reference)

        # Parts tree - compare sorted sets of node values (order within tree is deterministic
        # but may differ between Python and C++ traversal; sorted set is sufficient)
        ref_nodes = sorted(
            v for node in ri.parts.nodes for v in flatten_nodes(node)
        )
        test_nodes = sorted(
            v for node in ti.parts.nodes for v in flatten_nodes(node)
        )
        s.check(f"{key}: parts node count", len(ref_nodes), len(test_nodes))
        for idx, (rv, tv) in enumerate(zip(ref_nodes, test_nodes)):
            s.check(f"{key}: parts node[{idx}]", rv, tv)

    return s


def compare_rigid_groups(
    ref_joints: joint_pb2.Joints,
    test_joints: joint_pb2.Joints,
) -> Section:
    s = Section("RIGID GROUPS")
    ref_groups = {g.name: g for g in ref_joints.rigid_groups}
    test_groups = {g.name: g for g in test_joints.rigid_groups}

    diff_keys(s, set(ref_groups), set(test_groups), "rigid_groups")

    for name in sorted(set(ref_groups) & set(test_groups)):
        ro = sorted(ref_groups[name].occurrences)
        to = sorted(test_groups[name].occurrences)
        s.check(f"{name}: occurrence count", len(ro), len(to))
        for idx, (rv, tv) in enumerate(zip(ro, to)):
            s.check(f"{name}: occurrences[{idx}]", rv, tv)

    return s


def compare_appearances(
    ref_mats: material_pb2.Materials,
    test_mats: material_pb2.Materials,
) -> Section:
    s = Section("APPEARANCES")

    # Match by info.name - resolves the map-key format difference between exporters.
    # Skip "default" entries (hardcoded constants in both, not from Fusion data).
    ref_by_name = {
        a.info.name: a
        for a in ref_mats.appearances.values()
        if a.info.name.lower() != "default"
    }
    test_by_name = {
        a.info.name: a
        for a in test_mats.appearances.values()
        if a.info.name.lower() not in ("default", "default appearance")
    }

    diff_keys(s, set(ref_by_name), set(test_by_name), "appearances")

    for name in sorted(set(ref_by_name) & set(test_by_name)):
        ra = ref_by_name[name]
        ta = test_by_name[name]
        s.check(f"{name}: albedo.R", ra.albedo.R, ta.albedo.R)
        s.check(f"{name}: albedo.G", ra.albedo.G, ta.albedo.G)
        s.check(f"{name}: albedo.B", ra.albedo.B, ta.albedo.B)
        s.check(f"{name}: albedo.A", ra.albedo.A, ta.albedo.A)
        s.check(f"{name}: roughness", ra.roughness, ta.roughness, tol=FLOAT_TOL)
        s.check(f"{name}: metallic", ra.metallic, ta.metallic, tol=FLOAT_TOL)
        s.check(f"{name}: specular", ra.specular, ta.specular, tol=FLOAT_TOL)

    return s


def compare_physical_materials(
    ref_mats: material_pb2.Materials,
    test_mats: material_pb2.Materials,
) -> Section:
    s = Section("PHYSICAL MATERIALS")

    # Keys match between exporters (both use material.id).
    # Skip "default" hardcoded entries.
    ref_pm = {k: v for k, v in ref_mats.physicalMaterials.items() if k != "default"}
    test_pm = {k: v for k, v in test_mats.physicalMaterials.items() if k != "default"}

    diff_keys(s, set(ref_pm), set(test_pm), "physicalMaterials")

    for key in sorted(set(ref_pm) & set(test_pm)):
        rp = ref_pm[key]
        tp = test_pm[key]
        rm = rp.mechanical
        tm = tp.mechanical
        s.check(f"{key}: young_mod", rm.young_mod, tm.young_mod, tol=FLOAT_TOL)
        s.check(f"{key}: poisson_ratio", rm.poisson_ratio, tm.poisson_ratio, tol=FLOAT_TOL)
        s.check(f"{key}: shear_mod", rm.shear_mod, tm.shear_mod, tol=FLOAT_TOL)
        s.check(f"{key}: density", rm.density, tm.density, tol=FLOAT_TOL)
        rs = rp.strength
        ts = tp.strength
        s.check(f"{key}: yield_strength", rs.yield_strength, ts.yield_strength, tol=FLOAT_TOL)
        s.check(f"{key}: tensile_strength", rs.tensile_strength, ts.tensile_strength, tol=FLOAT_TOL)
        s.check(f"{key}: dynamic_friction", rp.dynamic_friction, tp.dynamic_friction, tol=FLOAT_TOL)
        s.check(f"{key}: static_friction", rp.static_friction, tp.static_friction, tol=FLOAT_TOL)
        s.check(f"{key}: restitution", rp.restitution, tp.restitution, tol=FLOAT_TOL)

    return s


def compare_signals(
    ref_sigs: signal_pb2.Signals,
    test_sigs: signal_pb2.Signals,
) -> Section:
    s = Section("SIGNALS")

    # Keys are random UUIDs per-export - match by info.name (which is the joint name).
    ref_by_name = {sig.info.name: sig for sig in ref_sigs.signal_map.values()}
    test_by_name = {sig.info.name: sig for sig in test_sigs.signal_map.values()}

    diff_keys(s, set(ref_by_name), set(test_by_name), "signals")

    for name in sorted(set(ref_by_name) & set(test_by_name)):
        rs = ref_by_name[name]
        ts = test_by_name[name]
        s.check(f"{name}: io", rs.io, ts.io)
        s.check(f"{name}: device_type", rs.device_type, ts.device_type)

    return s


def compare_design_hierarchy(
    ref_asm: assembly_pb2.Assembly,
    test_asm: assembly_pb2.Assembly,
) -> Section:
    s = Section("DESIGN HIERARCHY")

    ref_nodes = sorted(
        v for node in ref_asm.design_hierarchy.nodes for v in flatten_nodes(node)
    )
    test_nodes = sorted(
        v for node in test_asm.design_hierarchy.nodes for v in flatten_nodes(node)
    )

    s.check("node count", len(ref_nodes), len(test_nodes))
    for idx, (rv, tv) in enumerate(zip(ref_nodes, test_nodes)):
        s.check(f"node[{idx}]", rv, tv)

    return s


def compare_joint_hierarchy(
    ref_asm: assembly_pb2.Assembly,
    test_asm: assembly_pb2.Assembly,
) -> Section:
    s = Section("JOINT HIERARCHY")

    # C++ emits a flat tree (all joints under ground); Python builds a real tree.
    # We compare the set of node values only - structure may legitimately differ
    # for robots with multi-level joint chains.
    ref_nodes = sorted(
        v for node in ref_asm.joint_hierarchy.nodes for v in flatten_nodes(node)
    )
    test_nodes = sorted(
        v for node in test_asm.joint_hierarchy.nodes for v in flatten_nodes(node)
    )

    s.check("node count", len(ref_nodes), len(test_nodes))
    for idx, (rv, tv) in enumerate(zip(ref_nodes, test_nodes)):
        s.check(f"node[{idx}]", rv, tv)

    return s


def main() -> None:
    ap = argparse.ArgumentParser(
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    ap.add_argument("reference", help="Known good file to test against")
    ap.add_argument("test", help="Test file to verify")
    args = ap.parse_args()

    print(f"reference : {args.reference}")
    print(f"test      : {args.test}")

    ref_asm = load(args.reference)
    test_asm = load(args.test)

    sections = [
        compare_part_definitions(ref_asm.data, test_asm.data),
        compare_part_instances(ref_asm.data.parts, test_asm.data.parts),
        compare_joint_definitions(ref_asm.data.joints, test_asm.data.joints),
        compare_joint_instances(ref_asm.data.joints, test_asm.data.joints),
        compare_rigid_groups(ref_asm.data.joints, test_asm.data.joints),
        compare_appearances(ref_asm.data.materials, test_asm.data.materials),
        compare_physical_materials(ref_asm.data.materials, test_asm.data.materials),
        compare_signals(ref_asm.data.signals, test_asm.data.signals),
        compare_design_hierarchy(ref_asm, test_asm),
        compare_joint_hierarchy(ref_asm, test_asm),
    ]

    for sec in sections:
        sec.report()

    passed = sum(1 for sec in sections if sec.ok())
    total = len(sections)
    bar = "─" * 42
    print(f"\n{bar}")
    if passed == total:
        print(f"  PASSED  {passed}/{total} sections")
    else:
        failed = [sec.name for sec in sections if not sec.ok()]
        print(f"  FAILED  {passed}/{total} sections passed")
        print(f"  failing: {', '.join(failed)}")
    print(bar)

    sys.exit(0 if passed == total else 1)


if __name__ == "__main__":
    main()
