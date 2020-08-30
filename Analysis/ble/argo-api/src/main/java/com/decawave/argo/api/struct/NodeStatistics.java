/*
 * LEAPS - Low Energy Accurate Positioning System.
 *
 * Copyright (c) 2016-2017, LEAPS. All rights reserved.
 */

package com.decawave.argo.api.struct;

import java.util.Arrays;

/**
 * Raw statistics.
 *
 * Type mapping:
 *   uint32 -> long
 *   uint16 -> int
 *   int8 -> byte
 *   uint8 -> short
 *   float -> float
 *
 */
public class NodeStatistics {
    /* System */
    public long uptime;         // uint32
    public long memfree;        // uint32
    public float drift_avg_rtc; // float
    public byte mcu_temp;       // int8

    /* API */
    public byte api_err;        // int8
    public short api_err_cnt;   // uint8
    public byte api_dl_cnt;     // int8

    /* BLE */
    public int ble_con_ok;      // uint16
    public int ble_dis_ok;      // uint16
    public int ble_err;         // uint16
    public int clk_sync;        // uint16

    /* Interfaces */
    public long uwb0_intr;      // uint32
    public int uwb0_rst;        // uint16
    public int uwb0_bpc;        // uint16
    public long rx_ok;          // uint32
    public long rx_err;         // uint32
    public long tx_err;         // uint32
    public int tx_errx;         // uint16

    /* MAC */
    public int reinit;          // uint16
    public long alma_tx_ok;     // uint32
    public long alma_rx_ok;     // uint32
    public int alma_tx_err;     // uint16
    public int bcn_tx_err;      // uint16
    public long bcn_tx_ok;      // uint32
    public long bcn_rx_ok;      // uint32
    public int cl_tx_ok;        // uint16
    public int cl_rx_ok;        // uint16
    public int cl_coll;         // uint16
    public int fwup_tx_ok;      // uint16
    public int fwup_tx_err;     // uint16
    public int fwup_rx_ok;      // uint16
    public int svc_tx_err;      // uint16
    public int svc_tx_ok;       // uint16
    public int svc_rx_ok;       // uint16
    public int bh_ev;           // uint16

    public long bh_rt;          // uint32
    public long bh_nort;        // uint32
    public int bh_buf_lost[];   // uint16[]

    public long bh_tx_err;      // uint32
    public long bh_dl_err;      // uint32
    public long bh_dl_ok;       // uint32
    public long bh_ul_err;      // uint32
    public long bh_ul_ok;       // uint32
    public long fw_dl_tx_err;   // uint32
    public long fw_dl_iot_ok;   // uint32
    public long fw_ul_loc_ok;   // uint32
    public long fw_ul_iot_ok;   // uint32
    public long ul_tx_err;      // uint32
    public long dl_iot_ok;      // uint32
    public long ul_loc_ok;      // uint32
    public long ul_iot_ok;      // uint32
    public long sec_err;        // uint32

    /* Measurements */
    public long tdoa_ok;        // uint32
    public long tdoa_err;       // uint32
    public long twr_ok;         // uint32
    public long twr_err;        // uint32

    /* Reserved - max 6 elements */
    public long reserved[];     // uint32[6]

    public NodeStatistics() {
    }

    public NodeStatistics(NodeStatistics other) {
        this.uptime = other.uptime;
        this.memfree = other.memfree;
        this.drift_avg_rtc = other.drift_avg_rtc;
        this.mcu_temp = other.mcu_temp;
        this.api_err = other.api_err;
        this.api_err_cnt = other.api_err_cnt;
        this.api_dl_cnt = other.api_dl_cnt;
        this.ble_con_ok = other.ble_con_ok;
        this.ble_dis_ok = other.ble_dis_ok;
        this.ble_err = other.ble_err;
        this.clk_sync = other.clk_sync;
        this.uwb0_intr = other.uwb0_intr;
        this.uwb0_rst = other.uwb0_rst;
        this.uwb0_bpc = other.uwb0_bpc;
        this.rx_ok = other.rx_ok;
        this.rx_err = other.rx_err;
        this.tx_err = other.tx_err;
        this.tx_errx = other.tx_errx;
        this.reinit = other.reinit;
        this.alma_tx_ok = other.alma_tx_ok;
        this.alma_rx_ok = other.alma_rx_ok;
        this.alma_tx_err = other.alma_tx_err;
        this.bcn_tx_err = other.bcn_tx_err;
        this.bcn_tx_ok = other.bcn_tx_ok;
        this.bcn_rx_ok = other.bcn_rx_ok;
        this.cl_tx_ok = other.cl_tx_ok;
        this.cl_rx_ok = other.cl_rx_ok;
        this.cl_coll = other.cl_coll;
        this.fwup_tx_ok = other.fwup_tx_ok;
        this.fwup_tx_err = other.fwup_tx_err;
        this.fwup_rx_ok = other.fwup_rx_ok;
        this.svc_tx_err = other.svc_tx_err;
        this.svc_tx_ok = other.svc_tx_ok;
        this.svc_rx_ok = other.svc_rx_ok;
        this.bh_ev = other.bh_ev;
        this.bh_rt = other.bh_rt;
        this.bh_nort = other.bh_nort;
        if (other.bh_buf_lost != null) {
            //noinspection IncompleteCopyConstructor
            this.bh_buf_lost = Arrays.copyOf(other.bh_buf_lost, other.bh_buf_lost.length);
        }
        this.bh_tx_err = other.bh_tx_err;
        this.bh_dl_err = other.bh_dl_err;
        this.bh_dl_ok = other.bh_dl_ok;
        this.bh_ul_err = other.bh_ul_err;
        this.bh_ul_ok = other.bh_ul_ok;
        this.fw_dl_tx_err = other.fw_dl_tx_err;
        this.fw_dl_iot_ok = other.fw_dl_iot_ok;
        this.fw_ul_loc_ok = other.fw_ul_loc_ok;
        this.fw_ul_iot_ok = other.fw_ul_iot_ok;
        this.ul_tx_err = other.ul_tx_err;
        this.dl_iot_ok = other.dl_iot_ok;
        this.ul_loc_ok = other.ul_loc_ok;
        this.ul_iot_ok = other.ul_iot_ok;
        this.sec_err = other.sec_err;
        this.tdoa_ok = other.tdoa_ok;
        this.tdoa_err = other.tdoa_err;
        this.twr_ok = other.twr_ok;
        this.twr_err = other.twr_err;
        if (other.reserved != null) {
            //noinspection IncompleteCopyConstructor
            this.reserved = Arrays.copyOf(other.reserved, other.reserved.length);
        }
    }

    @Override
    public String toString() {
        return "NodeStatistics{" +
                "uptime=" + uptime +
                ", memfree=" + memfree +
                ", drift_avg_rtc=" + drift_avg_rtc +
                ", mcu_temp=" + mcu_temp +
                ", api_err=" + api_err +
                ", api_err_cnt=" + api_err_cnt +
                ", api_dl_cnt=" + api_dl_cnt +
                ", ble_con_ok=" + ble_con_ok +
                ", ble_dis_ok=" + ble_dis_ok +
                ", ble_err=" + ble_err +
                ", clk_sync=" + clk_sync +
                ", uwb0_intr=" + uwb0_intr +
                ", uwb0_rst=" + uwb0_rst +
                ", uwb0_bpc=" + uwb0_bpc +
                ", rx_ok=" + rx_ok +
                ", rx_err=" + rx_err +
                ", tx_err=" + tx_err +
                ", tx_errx=" + tx_errx +
                ", reinit=" + reinit +
                ", alma_tx_ok=" + alma_tx_ok +
                ", alma_rx_ok=" + alma_rx_ok +
                ", alma_tx_err=" + alma_tx_err +
                ", bcn_tx_err=" + bcn_tx_err +
                ", bcn_tx_ok=" + bcn_tx_ok +
                ", bcn_rx_ok=" + bcn_rx_ok +
                ", cl_tx_ok=" + cl_tx_ok +
                ", cl_rx_ok=" + cl_rx_ok +
                ", cl_coll=" + cl_coll +
                ", fwup_tx_ok=" + fwup_tx_ok +
                ", fwup_tx_err=" + fwup_tx_err +
                ", fwup_rx_ok=" + fwup_rx_ok +
                ", svc_tx_err=" + svc_tx_err +
                ", svc_tx_ok=" + svc_tx_ok +
                ", svc_rx_ok=" + svc_rx_ok +
                ", bh_ev=" + bh_ev +
                ", bh_rt=" + bh_rt +
                ", bh_nort=" + bh_nort +
                ", bh_buf_lost=" + Arrays.toString(bh_buf_lost) +
                ", bh_tx_err=" + bh_tx_err +
                ", bh_dl_err=" + bh_dl_err +
                ", bh_dl_ok=" + bh_dl_ok +
                ", bh_ul_err=" + bh_ul_err +
                ", bh_ul_ok=" + bh_ul_ok +
                ", fw_dl_tx_err=" + fw_dl_tx_err +
                ", fw_dl_iot_ok=" + fw_dl_iot_ok +
                ", fw_ul_loc_ok=" + fw_ul_loc_ok +
                ", fw_ul_iot_ok=" + fw_ul_iot_ok +
                ", ul_tx_err=" + ul_tx_err +
                ", dl_iot_ok=" + dl_iot_ok +
                ", ul_loc_ok=" + ul_loc_ok +
                ", ul_iot_ok=" + ul_iot_ok +
                ", sec_err=" + sec_err +
                ", tdoa_ok=" + tdoa_ok +
                ", tdoa_err=" + tdoa_err +
                ", twr_ok=" + twr_ok +
                ", twr_err=" + twr_err +
                ", reserved=" + Arrays.toString(reserved) +
                '}';
    }
}