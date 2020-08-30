/*
 * LEAPS - Low Energy Accurate Positioning System.
 *
 * Copyright (c) 2016-2017, LEAPS. All rights reserved.
 */

package com.decawave.argomanager.firmware;

import com.decawave.argo.api.struct.FirmwareMeta;
import com.decawave.argomanager.R;
import com.google.common.base.Objects;
import com.google.common.base.Preconditions;

/**
 * Argo project.
 */
@SuppressWarnings("ALL")
public class FirmwareRepository {

    public static final Firmware FW1_D14 = new Firmware(R.raw.dwm_core_fw1, new FirmwareMeta("1.3.0", 0xdeca002a, 0x01030000, 0x3D21C055 , 139008));
    public static final Firmware FW2_D14 = new Firmware(R.raw.dwm_core_fw2, new FirmwareMeta("1.3.0", 0xdeca002a, 0x01030001, 0xD5E94319 , 234040));

    public static final Firmware[] DEFAULT_FIRMWARE = new Firmware[] { FW1_D14, FW2_D14 };

    static {
        Preconditions.checkState(Objects.equal(DEFAULT_FIRMWARE[0].getMeta().tag, DEFAULT_FIRMWARE[1].getMeta().tag),
                "firmware tags do not match!");
    }

}
