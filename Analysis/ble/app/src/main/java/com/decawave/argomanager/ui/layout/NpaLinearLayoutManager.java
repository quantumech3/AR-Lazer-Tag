/*
 * LEAPS - Low Energy Accurate Positioning System.
 *
 * Copyright (c) 2016-2018, LEAPS. All rights reserved.
 */

package com.decawave.argomanager.ui.layout;

import android.content.Context;
import android.support.v7.widget.LinearLayoutManager;
import android.util.AttributeSet;

/**
 * Argo project.
 *
 * Non predictive animations linear layout manager.
 *
 * Fixing Android bug according to
 * https://stackoverflow.com/questions/30220771/recyclerview-inconsistency-detected-invalid-item-position
 */
public class NpaLinearLayoutManager extends LinearLayoutManager {

    public NpaLinearLayoutManager(Context context) {
        super(context);
    }

    public NpaLinearLayoutManager(Context context, int orientation, boolean reverseLayout) {
        super(context, orientation, reverseLayout);
    }

    public NpaLinearLayoutManager(Context context, AttributeSet attrs, int defStyleAttr, int defStyleRes) {
        super(context, attrs, defStyleAttr, defStyleRes);
    }

    @Override
    public boolean supportsPredictiveItemAnimations() {
        return false;
    }
}
