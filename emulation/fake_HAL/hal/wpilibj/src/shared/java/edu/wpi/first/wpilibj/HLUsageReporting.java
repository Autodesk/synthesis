/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.util.BaseSystemNotInitializedException;

/**
 * Support for high level usage reporting.
 */
@SuppressWarnings("JavadocMethod")
public class HLUsageReporting {
  private static Interface impl;

  @SuppressWarnings("MethodName")
  public static void SetImplementation(Interface implementation) {
    impl = implementation;
  }

  public static void reportScheduler() {
    if (impl != null) {
      impl.reportScheduler();
    } else {
      throw new BaseSystemNotInitializedException(Interface.class, HLUsageReporting.class);
    }
  }

  public static void reportPIDController(int num) {
    if (impl != null) {
      impl.reportPIDController(num);
    } else {
      throw new BaseSystemNotInitializedException(Interface.class, HLUsageReporting.class);
    }
  }

  public static void reportSmartDashboard() {
    if (impl != null) {
      impl.reportSmartDashboard();
    } else {
      throw new BaseSystemNotInitializedException(Interface.class, HLUsageReporting.class);
    }
  }

  public interface Interface {
    void reportScheduler();

    void reportPIDController(int num);

    void reportSmartDashboard();
  }

  public static class Null implements Interface {
    public void reportScheduler() {
    }

    @SuppressWarnings("PMD.UnusedFormalParameter")
    public void reportPIDController(int num) {
    }

    public void reportSmartDashboard() {
    }
  }
}
