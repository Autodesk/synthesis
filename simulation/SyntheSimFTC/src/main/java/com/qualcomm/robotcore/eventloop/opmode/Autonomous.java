package com.qualcomm.robotcore.eventloop.opmode;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * Clean-room shim of the real FTC SDK annotation.
 *
 * <p>{@link #preselectTeleOp()} is part of the real signature so team source
 * compiles unchanged; the harness itself ignores it, having no driver station
 * to preselect anything on.
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.TYPE)
public @interface Autonomous {
    String name() default "";

    String group() default "";

    String preselectTeleOp() default "";
}
