/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj;

/**
 * Interface for speed controlling devices.
 */
public interface SpeedController extends PIDOutput {

  /**
   * Common interface for getting the current set speed of a speed controller.
   *
   * @return The current set speed.  Value is between -1.0 and 1.0.
   */
  double get();

  /**
   * Common interface for setting the speed of a speed controller.
   *
   * @param speed The speed to set.  Value should be between -1.0 and 1.0.
   */
  void set(double speed);

  /**
   * Disable the speed controller
   */
  void disable();
}
