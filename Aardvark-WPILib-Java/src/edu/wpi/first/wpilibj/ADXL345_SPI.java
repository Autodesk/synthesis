/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008-2012. All Rights Reserved.                        */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/
package edu.wpi.first.wpilibj;


/**
 *
 * @author dtjones
 * @author mwills
 */
public class ADXL345_SPI extends SensorBase {
    private static final int kPowerCtlRegister = 0x2D;
    private static final int kDataFormatRegister = 0x31;
    private static final int kDataRegister = 0x32;
    private static final double kGsPerLSB = 0.00390625;
    
    private static final int kAddress_Read = 0x80;
    private static final int kAddress_MultiByte = 0x40;
    
    private static final int kPowerCtl_Link=0x20;
    private static final int kPowerCtl_AutoSleep=0x10;
    private static final int kPowerCtl_Measure=0x08;
    private static final int kPowerCtl_Sleep=0x04;
    
    private static final int kDataFormat_SelfTest=0x80;
    private static final int kDataFormat_SPI=0x40;
    private static final int kDataFormat_IntInvert=0x20;
    private static final int kDataFormat_FullRes=0x08;
    private static final int kDataFormat_Justify=0x04;
        
        
    public static class DataFormat_Range {

        /**
         * The integer value representing this enumeration
         */
        public final byte value;
        static final byte k2G_val = 0x00;
        static final byte k4G_val = 0x01;
        static final byte k8G_val = 0x02;
        static final byte k16G_val = 0x03;
        public static final ADXL345_SPI.DataFormat_Range k2G = new ADXL345_SPI.DataFormat_Range(k2G_val);
        public static final ADXL345_SPI.DataFormat_Range k4G = new ADXL345_SPI.DataFormat_Range(k4G_val);
        public static final ADXL345_SPI.DataFormat_Range k8G = new ADXL345_SPI.DataFormat_Range(k8G_val);
        public static final ADXL345_SPI.DataFormat_Range k16G = new ADXL345_SPI.DataFormat_Range(k16G_val);

        private DataFormat_Range(byte value) {
            this.value = value;
        }
    }

    public static class Axes {

        /**
         * The integer value representing this enumeration
         */
        public final byte value;
        static final byte kX_val = 0x00;
        static final byte kY_val = 0x02;
        static final byte kZ_val = 0x04;
        public static final ADXL345_SPI.Axes kX = new ADXL345_SPI.Axes(kX_val);
        public static final ADXL345_SPI.Axes kY = new ADXL345_SPI.Axes(kY_val);
        public static final ADXL345_SPI.Axes kZ = new ADXL345_SPI.Axes(kZ_val);

        private Axes(byte value) {
            this.value = value;
        }
    }

    public static class AllAxes {

        public double XAxis;
        public double YAxis;
        public double ZAxis;
    }
    
    private SPIDevice spi;
    
    /**
     * Constructor. Use this when the device is the first/only device on the bus
     *
     * @param clk The clock channel
     * @param mosi The mosi (output) channel
     * @param miso The miso (input) channel
     * @param cs The chip select channel
     * @param range The range (+ or -) that the accelerometer will measure.
     */
    public ADXL345_SPI(DigitalOutput clk, DigitalOutput mosi, DigitalInput miso, DigitalOutput cs, ADXL345_SPI.DataFormat_Range range) {
        spi = new SPIDevice(clk, mosi, miso, cs, SPIDevice.CS_ACTIVE_HIGH);
        init(range);
    }
    
    /**
     * Constructor. Use this when the device is the first/only device on the bus
     *
     * @param slot The module of the digital IO for the device
     * @param clkChannel The channel number for the clk channel
     * @param mosiChannel The channel number for the mosi (output) channel
     * @param misoChannel The channel number for the miso (input) channel
     * @param csChannel	The channel number for the chip select channel
     * @param range The range (+ or -) that the accelerometer will measure.
     */
    public ADXL345_SPI(int slot, int clkChannel, int mosiChannel, int misoChannel, int csChannel, ADXL345_SPI.DataFormat_Range range) {
        spi = new SPIDevice(slot, clkChannel, mosiChannel, misoChannel, csChannel, SPIDevice.CS_ACTIVE_HIGH);
        init(range);
    }

    /**
     * Constructor. Use this constructor only if another SPI device has already
     * been constructed with the clk, mosi and miso pins defined.
     *
     * @param cs the chip select line for the SPI bus
     * @param range The range (+ or -) that the accelerometer will measure.
     */
    public ADXL345_SPI(DigitalOutput cs, ADXL345_SPI.DataFormat_Range range) {
        spi = new SPIDevice(cs, SPIDevice.CS_ACTIVE_HIGH);
        init(range);
    }
    
    /**
     * Constructor. Use this constructor only if another SPI device has already
     * been constructed with the clk, mosi and miso pins defined.
     *
     * @param slot The module of the digital output for the device's chip select pin
     * @param csChannel	The channel for the digital output for the device's chip select pin
     * @param range The range (+ or -) that the accelerometer will measure.
     */
    public ADXL345_SPI(int slot, int csChannel, ADXL345_SPI.DataFormat_Range range) {
        spi = new SPIDevice(slot, csChannel, SPIDevice.CS_ACTIVE_HIGH);
        init(range);
    }
    
    public void free(){
        spi.free();
    }
    
    /**
     * Set SPI bus parameters, bring device out of sleep and set format
     * 
     * @param range The range (+ or -) that the accelerometer will measure.
     */
    private void init(ADXL345_SPI.DataFormat_Range range){
        spi.setBitOrder(SPIDevice.BIT_ORDER_MSB_FIRST);
        spi.setClockPolarity(SPIDevice.CLOCK_POLARITY_ACTIVE_LOW);
        spi.setDataOnTrailing(SPIDevice.DATA_ON_LEADING_EDGE);

        // Turn on the measurements
        spi.transfer((kPowerCtlRegister << 8) | kPowerCtl_Measure, 16);
        // Specify the data format to read
        spi.transfer((kDataFormatRegister << 8) | kDataFormat_FullRes | range.value, 16);
    }

    /**
     * Get the acceleration of one axis in Gs.
     *
     * @param axis The axis to read from.
     * @return Acceleration of the ADXL345 in Gs.
     */
    public double getAcceleration(ADXL345_SPI.Axes axis) {
        long rawAccelLong = spi.transfer( ((kAddress_Read | kAddress_MultiByte | kDataRegister) + axis.value) << 16, 24);
        return accelFromBytes(rawAccelLong);
    }

    private double accelFromBytes(long value) {
        // Sensor is little endian... swap bytes
        short rawAccel = (short) (value&0xFF);
        rawAccel <<= 8;
        rawAccel |= ((value>>8)&0xFF);
        return rawAccel * kGsPerLSB;
    }

    /**
     * Get the acceleration of all axes in Gs.
     *
     * @return Acceleration measured on all axes of the ADXL345 in Gs.
     */
    public ADXL345_SPI.AllAxes getAccelerations() {
        ADXL345_SPI.AllAxes data = new ADXL345_SPI.AllAxes();
        long[] rawData = spi.transfer(new long[]{kAddress_Read | kAddress_MultiByte | kDataRegister, 0, 0, 0}, new int[]{8, 16, 16, 16});

        data.XAxis = accelFromBytes(rawData[1]);
        data.YAxis = accelFromBytes(rawData[2]);
        data.ZAxis = accelFromBytes(rawData[3]);
        return data;
    }
}