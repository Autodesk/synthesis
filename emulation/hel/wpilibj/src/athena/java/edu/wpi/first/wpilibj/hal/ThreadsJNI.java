/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2016-2017. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.hal;

public class ThreadsJNI extends JNIWrapper {
  public static native int getCurrentThreadPriority();

  public static native boolean getCurrentThreadIsRealTime();

  public static native boolean setCurrentThreadPriority(boolean realTime, int priority);
}
