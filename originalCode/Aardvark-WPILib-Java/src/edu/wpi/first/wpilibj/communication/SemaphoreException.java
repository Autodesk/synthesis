/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.communication;

/**
 *
 * @author dtjones
 */
public class SemaphoreException extends Exception {
    public static final int M_semLib = 22 << 16;
    public static final int M_objLib = 61 << 16;

    public final static int S_objLib_OBJ_ID_ERROR = (M_objLib | 1);
    public final static int S_objLib_OBJ_UNAVAILABLE = (M_objLib | 2);
    public final static int S_objLib_OBJ_DELETED = (M_objLib | 3);
    public final static int S_objLib_OBJ_TIMEOUT = (M_objLib | 4);

    public final static int S_semLib_INVALID_STATE = (M_semLib | 101);
    public final static int S_semLib_INVALID_OPTION = (M_semLib | 102);
    public final static int S_semLib_INVALID_QUEUE_TYPE = (M_semLib | 103);
    public final static int S_semLib_INVALID_OPERATION = (M_semLib | 104);
    public final static int S_semLib_INVALID_INITIAL_COUNT = (M_semLib | 105);
    public final static int S_semLib_COUNT_OVERFLOW = (M_semLib | 106);

    /**
     * Generate a new SemaphoreException from the given status code.
     * @param status The status code describing the error.
     */
    public SemaphoreException (int status) {
        super(lookUpCode(status));
    }

    private static String lookUpCode(int status) {
        switch (status) {
            case S_objLib_OBJ_ID_ERROR:
                return "OBJ_ID_ERROR";

            case S_objLib_OBJ_UNAVAILABLE:
                return "OBJ_UNAVAILABLE";

            case S_objLib_OBJ_DELETED:
                return "OBJ_DELETED";

            case S_objLib_OBJ_TIMEOUT:
                return "OBJ_TIMEOUT";

            case S_semLib_INVALID_STATE:
                return "Invalid semaphore state";
            case S_semLib_INVALID_OPTION:
                return "Invalid semaphore option";
            case S_semLib_INVALID_QUEUE_TYPE:
                return "Invalid semaphore queue type";
            case S_semLib_INVALID_OPERATION:
                return "Invalid semaphore operation";
            case S_semLib_INVALID_INITIAL_COUNT:
                return "Invalid semaphore initial count";
            case S_semLib_COUNT_OVERFLOW:
                return "Semaphore count overflow";
            default:
                return "Unrecognized status code";
        }
    }
}
