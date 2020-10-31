package com.marin2.decawave.unity3dplugin;

import java.util.Date;

/**
 * Created by mhjtas on 19.1.2017.
 * Copyright: University of Turku & Mika Taskinen
 */
public class DecawavePacket {

    /**
     * Get the arrival timestamp of the current value
     * @return A unix timestamp in milliseconds (milliseconds from 1.1.1970)
     */
    public long getTimestampMillis() {
        return  timestamp.getTime();
    }

    /**
     * Get the arrival timestamp of the current value
     * @return A date/time object of timestamp
     */
    public Date getTimestamp() {
        return timestamp;
    }

    private final Date timestamp;

    /**
     * Get the id of the anchor
     * @return The id number of the anchor
     */
    public int getAnchorId() {
        return anchorId;
    }

    private final int anchorId;

    /**
     * Get the distance between beacon and anchor
     * @return The distance as millimeters
     */
    public int getDistanceInMillimeters() {
        return distanceInMillimeters;
    }

    private final int distanceInMillimeters;

    /**
     * Create a new DecawavePacket
     * @param anchorId The id of the anchor that sent the packet
     * @param distanceInMillimeters The distance from the anchor in millimeters
     */
    public DecawavePacket( int anchorId, int distanceInMillimeters ) {
        this.timestamp = new Date();
        this.anchorId = anchorId;
        this.distanceInMillimeters = distanceInMillimeters;
    }

}
