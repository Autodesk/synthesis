/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

package edu.wpi.first.wpilibj.communication;

import com.ni.rio.NiFpga;
import com.sun.jna.NativeLibrary;
import com.sun.jna.Pointer;

/**
 * Class exposing VxWorks semaphores.
 * @author dtjones
 */
public class Semaphore {
    Pointer m_semaphore;

    /** Integer ms value indicating wait forever. */
    public static final int WAIT_FOREVER = -1;

    static final int SEM_Q_FIFO                = 0x00; /* first in first out queue */
    static final int SEM_Q_PRIORITY            = 0x01; /* priority sorted queue */
    static final int SEM_DELETE_SAFE           = 0x04; /* owner delete safe (mutex opt.) */
    static final int SEM_INVERSION_SAFE        = 0x08; /* no priority inversion (mutex opt.) */
    static final int SEM_EVENTSEND_ERR_NOTIFY  = 0x10; /* notify when eventRsrcSend fails */
    static final int SEM_INTERRUPTIBLE         = 0x20; /* interruptible on RTP signal */

    /**
     * Options to create a semaphore with.
     */
    public static class Options {
        int value = 0;
        /**
         * Set true to use a priority sorted queue, false to use first-in first-out
         * @param priority
         */
        public void setPrioritySorted(boolean priority) {
            if (priority) value |= SEM_Q_PRIORITY;
            else value &= ~SEM_Q_PRIORITY;
        }
        /**
         * Set whether or not the semaphore is delete safe.
         * @param delSafe True to make the semaphore delete safe.
         */
        public void setDeleteSafe(boolean delSafe) {
            if (delSafe) value |= SEM_DELETE_SAFE;
            else value &= ~SEM_DELETE_SAFE;
        }
        /**
         * Set whether the semaphore is inversion safe.
         * @param invSafe True to set the semaphore to inversion safe.
         */
        public void setInversionSafe(boolean invSafe) {
            if (invSafe) value |= SEM_INVERSION_SAFE;
            else value &= ~SEM_INVERSION_SAFE;
        }
        /**
         * Set whether the semaphore should notify on an error.
         * @param errNot True to set error notify.
         */
        public void setErrorNotify(boolean errNot) {
            if (errNot) value |= SEM_EVENTSEND_ERR_NOTIFY;
            else value &= ~SEM_EVENTSEND_ERR_NOTIFY;
        }
        /**
         * Set whether the semaphore is interruptable.
         * @param intable True allows this semaphore to be interrupted.
         */
        public void setInterruptable(boolean intable) {
            if (intable) value |= SEM_INTERRUPTIBLE;
            else value &= ~SEM_INTERRUPTIBLE;
        }
    }

//   /* private static Function semMCreateFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semMCreate"); //SEM_ID 	  semMCreate 	(int options);
//    private static Function semBCreateFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semBCreate"); //SEM_ID 	  semBCreate 	(int options, SEM_B_STATE initialState);
//    private static Function semCCreateFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semCCreate"); //SEM_ID 	  semCCreate 	(int options, int initialCount);
//    private static Function semDeleteFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semDelete");  //STATUS 	  semDelete 	(SEM_ID semId);
//    private static Function semFlushFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semFlush");   //STATUS 	  semFlush 	(SEM_ID semId);
//    private static Function semGiveFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semGive");    //STATUS 	  semGive 	(SEM_ID semId);
//    private static Function semTakeFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semTake");    //STATUS 	  semTake 	(SEM_ID semId, int timeout);
//    private static Function semTakeBlockingFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getBlockingFunction("semTake");    //STATUS 	  semTake 	(SEM_ID semId, int timeout);
////    private static Function semOpenFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semOpen");    //SEM_ID	  semOpen	(const char * name, SEM_TYPE type, int initState, int options, int mode, void * context);
////    private static Function semInfoGetFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semInfoGet"); //STATUS	  semInfoGet	(SEM_ID semId, SEM_INFO *pInfo);
//    private static Function semCloseFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semClose");   //STATUS semClose (SEM_ID semId);
////    private static Function semUnlinkFn = NativeLibrary.getInstance(NiFpga.LIBRARY_NAME).getFunction("semUnlink");  //STATUS semUnlink (const char * name);
//
//    private void checkStatus (int status) throws SemaphoreException{
//        if (status != 0) {
//            throw new SemaphoreException(LibCUtil.errno());
//        }
//    }
//
//    /**
//     * Create a new semaphore.
//     * @param options The options to create the semaphore with.
//     */
//    public Semaphore (Options options) {
//        m_semaphore = new Pointer(semMCreateFn.call1(options.value), 0);
//    }
//
//    /**
//     * Create a semaphore with the given initial state.
//     * @param options The options to create the semaphore with.
//     * @param initialState The initial state for the semaphore to have.
//     */
//    public Semaphore (Options options, boolean initialState) {
//        m_semaphore = new Pointer(semBCreateFn.call2(options.value,initialState?1:0), 0);
//    }
//
//    /**
//     * Create a counting semaphore with the given value.
//     * @param options The options to create the semaphore with.
//     * @param count The initial count for the semaphore to hold.
//     */
//    public Semaphore (Options options, int count) {
//        m_semaphore = new Pointer(semCCreateFn.call2(options.value, count), 0);
//    }
//
//    /**
//     * Get the pointer to the native semaphore.
//     * @return Pointer to the native semaphore.
//     */
//    public Pointer getPointer() {
//        return m_semaphore;
//    }
//
//    /**
//     * Unblock every task that is blocked by the semaphore.
//     */
//    public void flush() throws SemaphoreException{
//        checkStatus(semFlushFn.call1(m_semaphore));
//    }
//
//    /**
//     * Release the semaphore.
//     */
//    public void give() throws SemaphoreException{
//        checkStatus(semGiveFn.call1(m_semaphore));
//    }
//
//    /**
//     * Take the semaphore. Block for timeout milliseconds.
//     * @param timeout The maximum time in milliseconds to block for the semaphore.
//     * @throws SemaphoreException if the lock can't be take in timeout seconds, or some other semaphore error condition occurs.
//     */
//    public void takeMillis(int timeout) throws SemaphoreException{
//        checkStatus(semTakeBlockingFn.call2(m_semaphore, timeout));
//    }
//
//    /**
//     * Take the semaphore. Block forever until semaphore is available.
//     * @throws SemaphoreException if some semaphore error condition occurs.
//     */
//    public void takeForever() throws SemaphoreException{
//        takeMillis(WAIT_FOREVER);
//    }
//
//    /*
//     * Non-blocking version of take(). Try to take the Semaphore.
//     * 
//     * @return If succeeded return true, otherwise return false.
//     * @throws SemaphoreException if some semaphore error condition occurs.
//     */
//    public boolean tryTake() throws SemaphoreException{
//        int result = semTakeFn.call2(m_semaphore, 0);
//        if (result == 0) {
//            return true;
//        } else {
//            int errno = LibCUtil.errno();
//            if (errno == SemaphoreException.S_objLib_OBJ_UNAVAILABLE) {
//                return false;
//            } else {
//                throw new SemaphoreException(errno);
//            }
//        }
//    }
//
//    /**
//     * Close the semaphore.
//     */
//    public void close () throws SemaphoreException{
//        checkStatus(semCloseFn.call1(m_semaphore));
//    }
//
//    /**
//     * Release all resources associated with the semaphore.
//     */
//    public void free () throws SemaphoreException{
//        checkStatus(semDeleteFn.call1(m_semaphore));
//    }
}
