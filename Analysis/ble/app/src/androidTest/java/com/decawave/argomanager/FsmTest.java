/*
 * LEAPS - Low Energy Accurate Positioning System.
 *
 * Copyright (c) 2016-2018, LEAPS. All rights reserved.
 */

package com.decawave.argomanager;

import android.os.Handler;
import android.os.Looper;
import android.support.test.runner.AndroidJUnit4;

import org.junit.Test;
import org.junit.runner.RunWith;

import java.util.concurrent.Semaphore;

import eu.kryl.android.common.fsm.FiniteStateMachine;
import eu.kryl.android.common.fsm.impl.FiniteStateMachineImpl;

/**
 * Argo project.
 */
@RunWith(AndroidJUnit4.class)
public class FsmTest {

    enum State {
        START,
        STOP
    }

    @Test
    public void testScheduleCurrentStateKeptRunnable() throws InterruptedException {
        Semaphore s = new Semaphore(0);
        Handler[] h = {null};
        Thread t = new Thread(() -> {
            try {
                Looper.prepare();
                System.out.println("setting HANDLER");
                h[0] = new Handler(Looper.myLooper());
            } catch (Exception e) {
                throw new RuntimeException(e);
            } finally {
                System.out.println("RELEASING");
                s.release();
            }
            System.out.println("LOOPING");
            Looper.loop();
        });
        // run the looper thread
        t.start();
        // wait for the handler to become available
        s.acquire();
        //
        System.out.println("POSTING on handler");
        h[0].post(() -> {
            FiniteStateMachine<State> fsm = new FiniteStateMachineImpl<>("test", State.class);
            fsm.getLog().setEnabled(true);
            fsm.addOnStateEnteredHandler(State.START, () -> {
                System.out.println("entered START now");
                fsm.scheduleRunnableForCurrentState(() -> {
                    System.out.println("still in START");
                });
            });
            fsm.addOnStateEnteredHandler(State.STOP, () -> {
                System.out.println("entered STOP now");
            });
            //
            System.out.println("setting state START");
            fsm.setState(State.START);
            System.out.println("setting state STOP");
            fsm.setState(State.STOP);
        });
        // wait for the test to complete
        Thread.sleep(1000);
    }

}
