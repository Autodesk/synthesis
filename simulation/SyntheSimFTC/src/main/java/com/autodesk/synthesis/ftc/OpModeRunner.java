package com.autodesk.synthesis.ftc;

import com.qualcomm.robotcore.eventloop.opmode.Autonomous;
import com.qualcomm.robotcore.eventloop.opmode.Disabled;
import com.qualcomm.robotcore.eventloop.opmode.LinearOpMode;
import com.qualcomm.robotcore.eventloop.opmode.OpModeManagerBridge;
import com.qualcomm.robotcore.eventloop.opmode.TeleOp;
import com.qualcomm.robotcore.hardware.HardwareMap;
import java.io.File;
import java.lang.reflect.Constructor;
import java.lang.reflect.Modifier;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import javax.tools.JavaCompiler;
import javax.tools.ToolProvider;

/**
 * Headless stand-in for FTC's OnBotJava: compiles a plain directory of team
 * source with the JDK's own compiler (no Gradle/Android project needed),
 * classloads the result, finds the @TeleOp/@Autonomous LinearOpMode by
 * reflection (same discovery mechanism the real SDK uses on-device), and drives
 * its lifecycle off Fission WS connect/disconnect events.
 */
public class OpModeRunner {
    public static void main(String[] args) throws Exception {
        Path srcDir = null;
        String opModeName = null;
        int port = FTCWsBridge.DEFAULT_PORT;

        for (int i = 0; i < args.length; i++) {
            switch (args[i]) {
                case "--src" -> srcDir = Path.of(args[++i]);
                case "--opmode" -> opModeName = args[++i];
                case "--port" -> port = Integer.parseInt(args[++i]);
                default -> throw new IllegalArgumentException("Unrecognized argument: " + args[i]);
            }
        }
        if (srcDir == null) {
            throw new IllegalArgumentException("Usage: OpModeRunner --src <directory> [--opmode <ClassName>] [--port <port>]");
        }

        OpModeSlots slots = compileAndDiscover(srcDir, opModeName);
        System.out.println("[OpModeRunner] TeleOp slot: " + slots.describe(slots.teleOp()));
        System.out.println("[OpModeRunner] Autonomous slot: " + slots.describe(slots.autonomous()));

        FTCWsBridge bridge = new FTCWsBridge(port);
        OpModeLifecycle lifecycle = new OpModeLifecycle(slots, bridge);
        bridge.setConnectionListener(lifecycle);
        bridge.start();
    }

    /** Owns spawning/stopping a fresh OpMode instance+thread per Fission connect/disconnect cycle. */
    private static class OpModeLifecycle implements FTCWsBridge.ConnectionListener {
        private final OpModeSlots slots;
        private final FTCWsBridge bridge;
        private OpModeCandidate running;
        private LinearOpMode current;
        private Thread thread;

        OpModeLifecycle(OpModeSlots slots, FTCWsBridge bridge) {
            this.slots = slots;
            this.bridge = bridge;
        }

        @Override
        public synchronized void onFissionConnected() {
            System.out.println("[OpModeRunner] Waiting for the driver station to enable");
        }

        @Override
        public synchronized void onFissionDisconnected() {
            stopCurrent();
        }

        @Override
        public synchronized void onDriverStationState(boolean enabled, boolean autonomous) {
            OpModeCandidate desired = !enabled ? null : autonomous ? slots.autonomous() : slots.teleOp();

            if (enabled && desired == null) {
                System.out.println("[OpModeRunner] Driver station enabled in "
                        + (autonomous ? "autonomous" : "teleop") + ", but no such OpMode was discovered");
            }

            if (desired != null && running != null && desired.cls() == running.cls()) {
                return;
            }

            stopCurrent();
            if (desired != null) {
                start(desired);
            }
        }

        private void start(OpModeCandidate candidate) {
            try {
                Constructor<? extends LinearOpMode> ctor = candidate.cls().getDeclaredConstructor();
                ctor.setAccessible(true);
                LinearOpMode opMode = ctor.newInstance();
                opMode.hardwareMap = new HardwareMap(this::createDevice);
                opMode.gamepad1 = bridge.gamepad1();
                opMode.gamepad2 = bridge.gamepad2();
                opMode.telemetry = new ConsoleTelemetry();

                opMode.resetRuntime();
                current = opMode;
                running = candidate;
                thread = new Thread(() -> {
                    try {
                        opMode.runOpMode();
                    } catch (InterruptedException e) {
                        Thread.currentThread().interrupt();
                    } catch (Exception e) {
                        System.err.println("[OpModeRunner] OpMode threw: " + e);
                        e.printStackTrace();
                    }
                }, "ftc-opmode");
                thread.start();
                OpModeManagerBridge.start(opMode);
                System.out.println("[OpModeRunner] Started [" + candidate.kind() + "] " + candidate.displayName());
            } catch (ReflectiveOperationException e) {
                throw new RuntimeException("Unable to construct " + candidate.cls().getName(), e);
            }
        }

        private void stopCurrent() {
            if (current == null) {
                return;
            }
            OpModeManagerBridge.stop(current);
            if (thread != null) {
                try {
                    thread.join(1000);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                }
            }
            System.out.println("[OpModeRunner] Stopped [" + running.kind() + "] " + running.displayName());
            current = null;
            running = null;
            thread = null;
        }

        private com.qualcomm.robotcore.hardware.HardwareDevice createDevice(Class<?> requestedType, String deviceName) {
            // Widen this as the shim grows past DcMotorSimple (Servo, CRServo, IMU, ...).
            if (requestedType.isAssignableFrom(SynthesisDcMotor.class)) {
                return new SynthesisDcMotor(deviceName, bridge);
            }
            return null;
        }
    }

    // A discovered OpMode. {@code displayName} is the annotation's name when the team set one
    private record OpModeCandidate(Class<? extends LinearOpMode> cls, boolean autonomous, String displayName) {
        String kind() {
            return autonomous ? "Autonomous" : "TeleOp";
        }

        boolean matches(String requested) {
            return cls.getSimpleName().equals(requested)
                    || cls.getName().equals(requested)
                    || displayName.equals(requested);
        }
    }

    //The one teleop and one autonomous the driver station can switch between without restarting the process.
    private record OpModeSlots(OpModeCandidate teleOp, OpModeCandidate autonomous) {
        String describe(OpModeCandidate candidate) {
            return candidate == null ? "(none)" : candidate.displayName() + " (" + candidate.cls().getName() + ")";
        }
    }

    private static OpModeSlots compileAndDiscover(Path srcDir, String requestedName) throws Exception {
        List<Path> sourceFiles;
        try (Stream<Path> walk = Files.walk(srcDir)) {
            sourceFiles = walk.filter(p -> p.toString().endsWith(".java")).collect(Collectors.toList());
        }
        if (sourceFiles.isEmpty()) {
            throw new IllegalArgumentException("No .java files found under " + srcDir);
        }

        Path outDir = Files.createTempDirectory("ftc-codesim-classes");
        JavaCompiler compiler = ToolProvider.getSystemJavaCompiler();
        if (compiler == null) {
            throw new IllegalStateException("No system Java compiler available -- run with a JDK, not a JRE");
        }

        List<String> compilerArgs = new ArrayList<>(List.of(
                "-d", outDir.toString(),
                "-cp", System.getProperty("java.class.path")));
        sourceFiles.forEach(p -> compilerArgs.add(p.toString()));

        int result = compiler.run(null, System.out, System.err, compilerArgs.toArray(new String[0]));
        if (result != 0) {
            throw new IllegalStateException("Compilation of " + srcDir + " failed (see errors above)");
        }

        URLClassLoader loader = new URLClassLoader(new URL[] {outDir.toUri().toURL()}, OpModeRunner.class.getClassLoader());

        List<OpModeCandidate> candidates = new ArrayList<>();
        try (Stream<Path> walk = Files.walk(outDir)) {
            for (Path classFile : (Iterable<Path>) walk.filter(p -> p.toString().endsWith(".class"))::iterator) {
                String relative = outDir.relativize(classFile).toString();
                String className = relative.substring(0, relative.length() - ".class".length())
                        .replace(File.separatorChar, '.');
                Class<?> cls = Class.forName(className, false, loader);
                OpModeCandidate candidate = asCandidate(cls);
                if (candidate != null) {
                    candidates.add(candidate);
                }
            }
        }

        if (candidates.isEmpty()) {
            throw new IllegalStateException("No @TeleOp or @Autonomous LinearOpMode class found under " + srcDir);
        }

        candidates.sort(Comparator.comparing(OpModeCandidate::displayName).thenComparing(c -> c.cls().getName()));
        candidates.forEach(c -> System.out.println(
                "[OpModeRunner] Discovered [" + c.kind() + "] " + c.displayName() + " (" + c.cls().getName() + ")"));

        return fillSlots(candidates, requestedName);
    }

    @SuppressWarnings("unchecked")
    private static OpModeCandidate asCandidate(Class<?> cls) {
        if (!LinearOpMode.class.isAssignableFrom(cls)
                || Modifier.isAbstract(cls.getModifiers())
                || cls.isAnnotationPresent(Disabled.class)) {
            return null;
        }

        TeleOp teleOp = cls.getAnnotation(TeleOp.class);
        Autonomous autonomous = cls.getAnnotation(Autonomous.class);
        if (teleOp == null && autonomous == null) {
            return null;
        }
        if (teleOp != null && autonomous != null) {
            throw new IllegalStateException(cls.getName() + " is annotated both @TeleOp and @Autonomous; pick one");
        }

        String annotated = teleOp != null ? teleOp.name() : autonomous.name();
        String displayName = annotated.isBlank() ? cls.getSimpleName() : annotated;
        return new OpModeCandidate((Class<? extends LinearOpMode>) cls, autonomous != null, displayName);
    }

    private static OpModeSlots fillSlots(List<OpModeCandidate> candidates, String requestedName) {
        OpModeCandidate requested = requestedName == null ? null : resolve(candidates, requestedName);
        return new OpModeSlots(slotFor(candidates, requested, false), slotFor(candidates, requested, true));
    }

    // Matches --opmode against class name, fully-qualified name, or annotation name.
    private static OpModeCandidate resolve(List<OpModeCandidate> candidates, String requestedName) {
        List<OpModeCandidate> matches = candidates.stream().filter(c -> c.matches(requestedName)).toList();
        if (matches.isEmpty()) {
            throw new IllegalArgumentException("No OpMode named " + requestedName + " found, discovered: "
                    + candidates.stream().map(OpModeCandidate::displayName).collect(Collectors.joining(", ")));
        }
        if (matches.size() > 1) {
            throw new IllegalArgumentException(requestedName + " is ambiguous, it matches: "
                    + matches.stream().map(c -> c.cls().getName()).collect(Collectors.joining(", ")));
        }
        return matches.get(0);
    }

    private static OpModeCandidate slotFor(List<OpModeCandidate> candidates, OpModeCandidate requested, boolean autonomous) {
        if (requested != null && requested.autonomous() == autonomous) {
            return requested;
        }

        List<OpModeCandidate> ofKind = candidates.stream().filter(c -> c.autonomous() == autonomous).toList();
        if (ofKind.isEmpty()) {
            return null;
        }
        if (ofKind.size() > 1) {
            System.out.println("[OpModeRunner] Multiple " + ofKind.get(0).kind() + " OpModes found, using "
                    + ofKind.get(0).displayName() + " -- pass --opmode to choose");
        }
        return ofKind.get(0);
    }
}
