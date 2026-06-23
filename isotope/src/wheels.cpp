#include "wheels.h"

#include <Core/UserInterface/UserInterface.h>

#include <algorithm>
#include <cmath>
#include <string>
#include <vector>

namespace {

// Two axes count as parallel/anti-parallel when |cos(angle)| is at least this.
constexpr float AXIS_PARALLEL_COS = 0.99f;
// The wheel-to-wheel segment must run along the axis: |cos(angle)| at least this.
constexpr float AXLE_ALIGN_COS = 0.98f;
// Origins closer than this (in meters) are treated as the same point, not a pair.
constexpr float COINCIDENT_DISTANCE = 1e-4f;
// Projected axle midpoints within this distance (1cm) of a line are collinear.
constexpr float COLLINEAR_DISTANCE = 0.01f;

struct Vec3 {
    float x, y, z;
};

Vec3 operator+(const Vec3& a, const Vec3& b) {
    return {a.x + b.x, a.y + b.y, a.z + b.z};
}

Vec3 operator-(const Vec3& a, const Vec3& b) {
    return {a.x - b.x, a.y - b.y, a.z - b.z};
}

Vec3 operator*(const Vec3& a, float s) {
    return {a.x * s, a.y * s, a.z * s};
}

float dot(const Vec3& a, const Vec3& b) {
    return a.x * b.x + a.y * b.y + a.z * b.z;
}

Vec3 cross(const Vec3& a, const Vec3& b) {
    return {a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x};
}

float norm(const Vec3& a) {
    return std::sqrt(dot(a, a));
}

Vec3 normalize(const Vec3& a) {
    float n = norm(a);
    if (n < 1e-9f) {
        return {0.0f, 0.0f, 0.0f};
    }

    return a * (1.0f / n);
}

Vec3 canonical_dir(const Vec3& v) {
    Vec3 n   = normalize(v);
    float ax = std::abs(n.x), ay = std::abs(n.y), az = std::abs(n.z);
    float dominant = (ax >= ay && ax >= az) ? n.x : (ay >= az ? n.y : n.z);
    return dominant < 0.0f ? n * -1.0f : n;
}

void perpendicular_basis(const Vec3& d, Vec3& u, Vec3& v) {
    Vec3 helper = (std::abs(d.x) < 0.9f) ? Vec3{1.0f, 0.0f, 0.0f} : Vec3{0.0f, 1.0f, 0.0f};
    u           = normalize(cross(d, helper));
    v           = cross(d, u); // unit length: d and u are orthonormal
}

struct Vec2 {
    float u, v;
};

// Perpendicular distance from a 2D point to the line through `anchor` with unit direction `dir`.
float distance_to_line(const Vec2& p, const Vec2& anchor, const Vec2& dir) {
    Vec2 ap = {p.u - anchor.u, p.v - anchor.v};
    return std::abs(ap.u * dir.v - ap.v * dir.u);
}

struct RevoluteJointCandidate {
    std::string token;
    Vec3 origin;
    Vec3 axis; // normalized rotation axis
};

struct AxlePair {
    size_t a, b; // indices into the candidate list
    Vec3 midpoint; // halfway between the two joint origins
    Vec3 direction; // canonical axle direction
    std::string key; // sorted "tokenA|tokenB" for deterministic ordering
};

struct DirectionGroup {
    Vec3 direction;
    std::vector<size_t> pairs; // indices into the axle-pair list
};

std::vector<RevoluteJointCandidate> extract_revolute_candidates(const mirabuf::joint::Joints* joints) {
    std::vector<RevoluteJointCandidate> candidates;

    for (const auto& [token, joint_def] : joints->joint_definitions()) {
        if (joint_def.joint_motion_type() != mirabuf::joint::JointMotion::REVOLUTE) {
            continue;
        }

        if (!joint_def.has_rotational()) {
            continue;
        }

        const auto& a = joint_def.rotational().rotational_freedom().axis();
        Vec3 axis     = normalize({static_cast<float>(a.x()), static_cast<float>(a.y()), static_cast<float>(a.z())});
        if (norm(axis) < 0.5f) {
            continue; // degenerate / unset axis
        }

        const auto& o = joint_def.origin();
        candidates.push_back(
            {token, {static_cast<float>(o.x()), static_cast<float>(o.y()), static_cast<float>(o.z())}, axis});
    }

    std::sort(candidates.begin(), candidates.end(), [](const auto& l, const auto& r) { return l.token < r.token; });
    return candidates;
}

std::vector<AxlePair> build_axle_pairs(const std::vector<RevoluteJointCandidate>& candidates) {
    std::vector<AxlePair> pairs;

    for (size_t i = 0; i < candidates.size(); ++i) {
        for (size_t j = i + 1; j < candidates.size(); ++j) {
            if (std::abs(dot(candidates[i].axis, candidates[j].axis)) < AXIS_PARALLEL_COS) {
                continue;
            }

            Vec3 displacement = candidates[j].origin - candidates[i].origin;
            float span        = norm(displacement);
            if (span < COINCIDENT_DISTANCE) {
                continue; // same point, not an axle
            }

            Vec3 axle_dir = displacement * (1.0f / span);
            if (std::abs(dot(axle_dir, candidates[i].axis)) < AXLE_ALIGN_COS) {
                continue;
            }

            const std::string& ta = candidates[i].token;
            const std::string& tb = candidates[j].token;
            pairs.push_back({i, j, (candidates[i].origin + candidates[j].origin) * 0.5f,
                canonical_dir(candidates[i].axis), ta < tb ? ta + "|" + tb : tb + "|" + ta});
        }
    }

    std::sort(pairs.begin(), pairs.end(), [](const auto& l, const auto& r) { return l.key < r.key; });
    return pairs;
}

// Bucket axle pairs whose directions are parallel or anti-parallel.
std::vector<DirectionGroup> group_by_direction(const std::vector<AxlePair>& pairs) {
    std::vector<DirectionGroup> groups;

    for (size_t i = 0; i < pairs.size(); ++i) {
        bool placed = false;
        for (auto& group : groups) {
            if (std::abs(dot(group.direction, pairs[i].direction)) >= AXIS_PARALLEL_COS) {
                group.pairs.push_back(i);
                placed = true;
                break;
            }
        }

        if (!placed) {
            groups.push_back({pairs[i].direction, {i}});
        }
    }

    return groups;
}

// Largest subset of points lying on a single line, by index into `points`.
// Two or fewer points are trivially collinear. Ties keep the first line found
// (deterministic because `points` arrives in sorted-pair order) and set `tied`.
std::vector<size_t> largest_collinear_set(const std::vector<Vec2>& points, bool& tied) {
    std::vector<size_t> best;
    tied = false;

    if (points.size() <= 2) {
        for (size_t i = 0; i < points.size(); ++i) {
            best.push_back(i);
        }

        return best;
    }

    for (size_t i = 0; i < points.size(); ++i) {
        for (size_t j = i + 1; j < points.size(); ++j) {
            Vec2 dir  = {points[j].u - points[i].u, points[j].v - points[i].v};
            float len = std::sqrt(dir.u * dir.u + dir.v * dir.v);
            if (len < 1e-9f) {
                continue;
            }

            dir = {dir.u / len, dir.v / len};
            std::vector<size_t> on_line;
            for (size_t k = 0; k < points.size(); ++k) {
                if (distance_to_line(points[k], points[i], dir) <= COLLINEAR_DISTANCE) {
                    on_line.push_back(k);
                }
            }

            if (on_line.size() > best.size()) {
                tied = false;
                best = std::move(on_line);
            } else if (!best.empty() && on_line.size() == best.size()) {
                tied = true;
            }
        }
    }

    return best;
}

} // namespace

void detect_and_tag_wheels(mirabuf::joint::Joints* joints, const adsk::core::Ptr<adsk::fusion::Design>& design,
    const adsk::core::Ptr<adsk::core::UserInterface>& ui) {
    if (!joints) {
        return;
    }

    auto candidates = extract_revolute_candidates(joints);
    if (candidates.size() < 2) {
        return;
    }

    auto pairs = build_axle_pairs(candidates);
    if (pairs.empty()) {
        return;
    }

    auto groups = group_by_direction(pairs);

    // Across all axle-direction groups, keep the largest collinear drivetrain.
    std::vector<size_t> selected; // indices into `pairs`
    bool tied = false;
    for (const auto& group : groups) {
        Vec3 u, v;
        perpendicular_basis(group.direction, u, v);

        std::vector<Vec2> midpoints;
        for (size_t idx : group.pairs) {
            midpoints.push_back({dot(pairs[idx].midpoint, u), dot(pairs[idx].midpoint, v)});
        }

        bool group_tied = false;
        auto collinear  = largest_collinear_set(midpoints, group_tied);
        if (collinear.size() > selected.size()) {
            tied = group_tied;
            std::vector<size_t> resolved;
            for (size_t local : collinear) {
                resolved.push_back(group.pairs[local]);
            }

            selected = std::move(resolved);
        } else if (!selected.empty() && collinear.size() == selected.size()) {
            tied = true;
        }
    }

    if (tied && ui) {
        ui->messageBox("Warning: multiple equally valid drivetrains were detected. "
                       "One was selected automatically using a deterministic ordering. ",
            "Wheel Detection Ambiguity");
    }

    if (selected.empty()) {
        return;
    }

    auto tag = [&](const std::string& token) {
        auto& joint_def = joints->mutable_joint_definitions()->at(token);
        joint_def.mutable_user_data()->mutable_data()->insert({"wheel", "true"});
        joint_def.mutable_user_data()->mutable_data()->insert({"wheelType", "0"});
    };

    for (size_t idx : selected) {
        tag(candidates[pairs[idx].a].token);
        tag(candidates[pairs[idx].b].token);
    }
}
