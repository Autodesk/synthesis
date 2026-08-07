package com.autodesk.synthesis.ftc;

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
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import javax.tools.JavaCompiler;
import javax.tools.ToolProvider;

/**
 * Headless stand-in for FTC's OnBotJava: compiles a plain directory of team
 * source with the JDK's own compiler (no Gradle/Android project needed),
 * classloads the result, finds the @TeleOp LinearOpMode by reflection and 
 * drives its lifecycle off Fission WS connect/disconnect events.
 */
public class OpModeRunner {
    public static void main(String[] args) throws Exception {
        Path srcDir = null;
        String opModeName = null;

        for (int i = 0; i < args.length; i++) {
            switch (args[i]) {
                case "--src" -> srcDir = Path.of(args[++i]);
                case "--opmode" -> opModeName = args[++i];
                default -> throw new IllegalArgumentException("Unrecognized argument: " + args[i]);
            }
        }
        if (srcDir == null) {
            throw new IllegalArgumentException("Usage: OpModeRunner --src <directory> [--opmode <ClassName>]");
        }

        Class<? extends LinearOpMode> opModeClass = compileAndDiscover(srcDir, opModeName);
        System.out.println("[OpModeRunner] Running " + opModeClass.getName());

        FTCWsBridge bridge = new FTCWsBridge(FTCWsBridge.DEFAULT_PORT);
        OpModeLifecycle lifecycle = new OpModeLifecycle(opModeClass, bridge);
        bridge.setConnectionListener(lifecycle);
        bridge.start();
    }

    /** Owns spawning/stopping a fresh OpMode instance+thread per Fission connect/disconnect cycle. */
    private static class OpModeLifecycle implements FTCWsBridge.ConnectionListener {
        private final Class<? extends LinearOpMode> opModeClass;
        private final FTCWsBridge bridge;
        private volatile LinearOpMode current;
        private volatile Thread thread;

        OpModeLifecycle(Class<? extends LinearOpMode> opModeClass, FTCWsBridge bridge) {
            this.opModeClass = opModeClass;
            this.bridge = bridge;
        }

        @Override
        public synchronized void onFissionConnected() {
            try {
                Constructor<? extends LinearOpMode> ctor = opModeClass.getDeclaredConstructor();
                ctor.setAccessible(true);
                LinearOpMode opMode = ctor.newInstance();
                opMode.hardwareMap = new HardwareMap(this::createDevice);
                opMode.gamepad1 = bridge.gamepad1();
                opMode.gamepad2 = bridge.gamepad2();
                opMode.telemetry = new ConsoleTelemetry();

                current = opMode;
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
                bridge.setEnabled(true);
            } catch (ReflectiveOperationException e) {
                throw new RuntimeException("Unable to construct " + opModeClass.getName(), e);
            }
        }

        @Override
        public synchronized void onFissionDisconnected() {
            bridge.setEnabled(false);
            if (current != null) {
                OpModeManagerBridge.stop(current);
            }
            if (thread != null) {
                try {
                    thread.join(1000);
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt();
                }
            }

            current = null;
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

    private static Class<? extends LinearOpMode> compileAndDiscover(Path srcDir, String requestedName) throws Exception {
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

        List<Class<? extends LinearOpMode>> candidates = new ArrayList<>();
        try (Stream<Path> walk = Files.walk(outDir)) {
            for (Path classFile : (Iterable<Path>) walk.filter(p -> p.toString().endsWith(".class"))::iterator) {
                String relative = outDir.relativize(classFile).toString();
                String className = relative.substring(0, relative.length() - ".class".length())
                        .replace(File.separatorChar, '.');
                Class<?> cls = Class.forName(className, false, loader);
                if (LinearOpMode.class.isAssignableFrom(cls)
                        && !Modifier.isAbstract(cls.getModifiers())
                        && cls.isAnnotationPresent(TeleOp.class)
                        && !cls.isAnnotationPresent(Disabled.class)) {
                    candidates.add((Class<? extends LinearOpMode>) cls);
                }
            }
        }

        if (candidates.isEmpty()) {
            throw new IllegalStateException("No @TeleOp LinearOpMode class found under " + srcDir);
        }

        if (requestedName != null) {
            return candidates.stream()
                    .filter(c -> c.getSimpleName().equals(requestedName) || c.getName().equals(requestedName))
                    .findFirst()
                    .orElseThrow(() -> new IllegalArgumentException("No @TeleOp class named " + requestedName + " found"));
        }

        if (candidates.size() > 1) {
            System.out.println("[OpModeRunner] Multiple @TeleOp classes found, using the first: "
                    + candidates.stream().map(Class::getName).collect(Collectors.joining(", ")));
        }

        return candidates.get(0);
    }
}
