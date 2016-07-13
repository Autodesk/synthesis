//Inspired by http://www.chiefdelphi.com/forums/showthread.php?t=97885&highlight=SPI
package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.communication.UsageReporting;
import edu.wpi.first.wpilibj.fpga.tDIO;
import edu.wpi.first.wpilibj.fpga.tSPI;

/**
 *
 * Represents a device on an SPI bus
 * Note that the cRIO only supports one SPI bus
 * Attempting to open a second SPI device with a
 * different shared pin (clk, mosi, miso) will
 * result in an exception
 *
 * @author mwils
 */
public class SPIDevice extends SensorBase {

    //used to synchronize access to the SPI bus
    private static final Object semaphore = new Object();
    //tSPI instance
    private static tSPI spi = null;
    private static boolean createdBusChannels = false;
    private static DigitalOutput clkChannel = null;
    private static DigitalOutput mosiChannel = null;
    private static DigitalInput misoChannel = null;
    private static int devices = 0;

    /**
     * Initialize SPI bus<br>
     * Only call this method once in the program
     *
     * @param clk	The digital output for the clock signal.
     * @param mosi	The digital output for the written data to the slave
     * (master-out slave-in).
     * @param miso	The digital input for the input data from the slave
     * (master-in slave-out).
     */
    private static void initBus(final DigitalOutput clk, final DigitalOutput mosi, final DigitalInput miso) {
        if (spi == null)
            spi = new tSPI();
        else if(clk.getChannel() == clkChannel.getChannel() && mosi.getChannel() == mosiChannel.getChannel()
                && miso.getChannel() == misoChannel.getChannel())
            return;
        else
            throw new BadSPIConfigException("SPI bus already configured with different clk, mosi and/or miso");
                    
        clkChannel = clk;
        mosiChannel = mosi;
        misoChannel = miso;


        tSPI.writeChannels_SCLK_Module(clk.getModuleForRouting());
        tSPI.writeChannels_SCLK_Channel(clk.getChannelForRouting());


        if (mosi != null) {
            tSPI.writeChannels_MOSI_Module(mosi.getModuleForRouting());
            tSPI.writeChannels_MOSI_Channel(mosi.getChannelForRouting());
        } else {
            tSPI.writeChannels_MOSI_Module(0);
            tSPI.writeChannels_MOSI_Channel(0);
        }

        if (miso != null) {
            tSPI.writeChannels_MISO_Module(miso.getModuleForRouting());
            tSPI.writeChannels_MISO_Channel(miso.getChannelForRouting());
            tSPI.writeConfig_WriteOnly(false);//TODO check these are right
        } else {
            tSPI.writeChannels_MISO_Module(0);
            tSPI.writeChannels_MISO_Channel(0);
            tSPI.writeConfig_WriteOnly(true);
        }


        tSPI.writeChannels_SS_Module(0);
        tSPI.writeChannels_SS_Channel(0);

        tSPI.strobeReset();
        tSPI.strobeClearReceivedData();
    }

    /**
     * Cleanup the SPI Bus
     */
    private static void freeBus(boolean freeChannels) {
        if (spi != null) {
            spi.Release();
            spi = null;
        }
        if(freeChannels){
            clkChannel.free();
            mosiChannel.free();
            misoChannel.free();
        }
}
	
    /**
     * Perform a SPI transfer with the length of the selected device's current
     * configuration
     *
     * @param value	The value to write to the bus
     */
    private static long trasferStatic(long value) {
        synchronized (semaphore) {
            tSPI.strobeClearReceivedData();

            tSPI.writeDataToLoad(value);
            tSPI.strobeLoad();
            if (!tSPI.readConfig_WriteOnly())
            {
                while (tSPI.readReceivedElements() == 0) {
                    Thread.yield();

                }

                tSPI.strobeReadReceivedData();
                return tSPI.readReceivedData();
            }
            else
                return(0);
        }
    }

    /**
     * When transferring data the it is sent and received with the most
     * significant bit first
     *
     * @see #setBitOrder(boolean)
     */
    public static final boolean BIT_ORDER_MSB_FIRST = true;
    /**
     * When transferring data the it is sent and received with the least
     * significant bit first
     *
     * @see #setBitOrder(boolean)
     */
    public static final boolean BIT_ORDER_LSB_FIRST = false;
    /**
     * When transferring data the clock will be active high<br> This corresponds
     * to CPOL=0
     *
     * @see #setClockPolarity(boolean)
     */
    public static final boolean CLOCK_POLARITY_ACTIVE_HIGH = false;
    /**
     * When transferring data the clock will be active low<br> This corresponds
     * to CPOL=1
     *
     * @see #setClockPolarity(boolean)
     */
    public static final boolean CLOCK_POLARITY_ACTIVE_LOW = true;
    /**
     * Data is valid on the leading edge of the clock pulse<br> This corresponds
     * to CPHA=0
     *
     * @see #setDataOnFalling(boolean)
     */
    public static final boolean DATA_ON_LEADING_EDGE = false;
    /**
     * Data is valid on the trailing edge of the clock pulse<br> This
     * corresponds to CPHA=1
     *
     * @see #setDataOnFalling(boolean)
     */
    public static final boolean DATA_ON_TRAILING_EDGE = true;
    /**
     * The CS will be brought high when the device is selected
     */
    public static final boolean CS_ACTIVE_HIGH = true;
    /**
     * The CS will be brought low when the device is selected
     */
    public static final boolean CS_ACTIVE_LOW = false;
    /**
     * The chip select line is active for the duration of the frame. 
     * 
     * @see #setFrameMode(char) 
     */
    public static final char FRAME_MODE_CHIP_SELECT = 0;
    /**
     * The chip select line pulses before the transfer of each frame. 
     * 
     * @see #setFrameMode(char) 
     */
    public static final char FRAME_MODE_PRE_LATCH = 1;
    /**
     * The chip select line pulses after the transfer of each frame.
     * 
     * @see #setFrameMode(char) 
     */
    public static final char FRAME_MODE_POST_LATCH = 2;
    /**
     * The chip select line pulses before and after each frame. 
     * 
     * @see #setFrameMode(char) 
     */
    public static final char FRAME_MODE_PRE_POST_LATCH_PULSE = 3;
    
    private boolean createdCSChannel = false;
    private DigitalOutput cs = null;
    
    private boolean csActiveHigh;
    private boolean bitOrder = BIT_ORDER_MSB_FIRST;
    private boolean clockPolarity = CLOCK_POLARITY_ACTIVE_HIGH;
    private boolean dataOnTrailing = DATA_ON_LEADING_EDGE;
    private int clockHalfPeriodDelay = 0;//fastest clockrate possible
    private char frameMode = FRAME_MODE_CHIP_SELECT;
    /**
     * The maximum rate the clock can transmit at
     */
    public final double MAX_CLOCK_FREQUENCY = 1/(2*tDIO.readLoopTiming()/(kSystemClockTicksPerMicrosecond*1e6));
    /**
     * The minimum rate the clock can transmit at
     */
    public final double MIN_CLOCK_FREQUENCY = 1/255*(2*tDIO.readLoopTiming()/(kSystemClockTicksPerMicrosecond*1e6));
    
    /**
     * Create a new device on the SPI bus.
     * The chip select line is active low
     *
     * @param slot The module of the digital IO for the device
     * @param clkChannel The channel number for the clk channel
     * @param mosiChannel The channel number for the mosi (output) channel
     * @param misoChannel The channel number for the miso (input) channel
     * @param csChannel	The channel number for the chip select channel
     */
    public SPIDevice(int slot, int clkChannel, int mosiChannel, int misoChannel, int csChannel) {
        this(new DigitalOutput(slot, clkChannel), new DigitalOutput (slot, mosiChannel), 
                new DigitalInput(slot, misoChannel), new DigitalOutput(slot, csChannel), CS_ACTIVE_LOW, true);
    }
    
    /**
     * Create a new device on the SPI bus.
     *
     * @param slot The module of the digital IO for the device
     * @param clkChannel The channel number for the clk channel
     * @param mosiChannel The channel number for the mosi (output) channel
     * @param misoChannel The channel number for the miso (input) channel
     * @param csChannel	The channel number for the chip select channel
     * @param csActiveHigh True if the chip select line should be high when
     *          the device is selected. False if it should be low.
     */
    public SPIDevice(int slot, int clkChannel, int mosiChannel, int misoChannel, int csChannel, boolean csActiveHigh) {
        this(new DigitalOutput(slot, clkChannel), new DigitalOutput (slot, mosiChannel), 
                new DigitalInput(slot, misoChannel), new DigitalOutput(slot, csChannel), csActiveHigh, true);
    }
    
    /**
     * Create a new device on the SPI bus.
     * The chip select line is active low
     *
     * @param clk The clock channel
     * @param mosi The mosi (output) channel
     * @param miso The miso (input) channel
     * @param cs The chip select channel
     */
    public SPIDevice(DigitalOutput clk, DigitalOutput mosi, DigitalInput miso, DigitalOutput cs) {
        this(clk, mosi, miso, cs, CS_ACTIVE_LOW, false);
    } 
    
    /**
     * Create a new device on the SPI bus.
     *
     * @param clk The clock channel
     * @param mosi The mosi (output) channel
     * @param miso The miso (input) channel
     * @param cs The chip select channel
     * @param csActiveHigh True if the chip select line should be high when
     *          the device is selected. False if it should be low.
     */
    public SPIDevice(DigitalOutput clk, DigitalOutput mosi, DigitalInput miso, DigitalOutput cs, boolean csActiveHigh) {
        this(clk, mosi, miso, cs, csActiveHigh, false);
    }    
    
    /**
     * Create a new device on the SPI bus.
     * Must only be used after a device has been created establishing the 
     * clk, mosi and miso channels. The chip select line is active low
     *
     * @param slot The module of the digital output for the device's chip select pin
     * @param csChannel	The channel for the digital output for the device's chip select pin
     */
    public SPIDevice(int slot, int csChannel) {
        this(new DigitalOutput(slot, csChannel), CS_ACTIVE_LOW, true);
    }
    
    /**
     * Create a new device on the SPI bus.
     * Must only be used after a device has been created establishing the 
     * clk, mosi and miso channels.
     *
     * @param slot The module of the digital output for the device's chip select pin
     * @param csChannel	The channel for the digital output for the device's chip select pin
     * @param csActiveHigh True if the chip select line should be high when
     *          the device is selected. False if it should be low.
     */
    public SPIDevice(int slot, int csChannel, boolean csActiveHigh) {
        this(new DigitalOutput(slot, csChannel), csActiveHigh, true);
    }

    /**
     * Create a new device on the SPI bus.
     * Must only be used after a device has been created establishing the 
     * clk, mosi and miso channels. The chip select line is active low
     *
     * @param cs The chip select channel
     */
    public SPIDevice(DigitalOutput cs) {
        this(cs, CS_ACTIVE_LOW);
    }

    /**
     * Create a new device on the SPI bus.
     * Must only be used after a device has been created establishing the 
     * clk, mosi and miso channels.
     *
     * @param cs The chip select channel
     * @param csActiveHigh True if the chip select line should be high when
     *          the device is selected. False if it should be low.
     */
    public SPIDevice(DigitalOutput cs, boolean csActiveHigh) {
        this(cs, csActiveHigh, false);
    }
    
    /**
     * Create a new device on the SPI bus.
     *
     * @param clk The clock channel
     * @param mosi The mosi (output) channel
     * @param miso The miso (input) channel
     * @param cs The chip select channel
     * @param csActiveHigh True if the chip select line should be high when
     *          the device is selected. False if it should be low.
     * @param createdChannel True if this class owns the DigitalInput/Output objects
     */
    private SPIDevice(DigitalOutput clk, DigitalOutput mosi, DigitalInput miso, 
            DigitalOutput cs, boolean csActiveHigh, boolean createdChannel) {
        initBus(clk, mosi, miso);
        devices += 1;
        this.createdBusChannels = createdChannel;
        this.createdCSChannel = createdChannel;
        this.cs = cs;
        this.csActiveHigh = csActiveHigh;
        cs.set(!csActiveHigh);
        
        UsageReporting.report(UsageReporting.kResourceType_SPI, devices);
    }

    /**
     * Create a new device on the SPI bus.
     * Must only be used after a device has been created establishing the 
     * clk, mosi and miso channels.
     *
     * @param slot The module of the digital output for the device's chip select pin
     * @param csChannel	The channel for the digital output for the device's chip select pin
     * @param csActiveHigh True if the chip select line should be high when
     *          the device is selected. False if it should be low.
     * @param createdChannel True if this class owns the DigitalInput/Output objects
     */
    private SPIDevice(DigitalOutput cs, boolean csActiveHigh, boolean createdChannel) {
        if (spi == null) {
            throw new RuntimeException("Must create SPI with clk, miso and mosi first");
        }
        devices += 1;
        this.createdCSChannel = createdChannel;
        this.cs = cs;
        this.csActiveHigh = csActiveHigh;
        cs.set(!csActiveHigh);
        
        UsageReporting.report(UsageReporting.kResourceType_SPI, devices);
    }
    
    /**
     * Free the resources used by this object
     */
    public void free(){
        if(createdCSChannel && cs!=null)
            cs.free();
        devices -= 1;
        if (devices == 0)
            freeBus(createdBusChannels);
    }

    /**
     * Perform a SPI transfer with the length of this device's current
     * configuration. This will select the device, transfer the data and then
     * deselect the device
     *
     * @param writeValue	The value to write to the device
     * @param numBits	The number of bits to write/read
     */
    public long transfer(long writeValue, int numBits) {
        long[] readValue;
        synchronized (semaphore) {
            readValue = transfer(new long[]{writeValue}, new int[]{numBits});
        }
        return readValue[0];
    }

    /**
     * Perform a SPI transfer where an array of bits are written and read. The
     * number of bits to write and read is specified in numBits<br> The whole
     * transfer will occur with the cs line held active throughout
     *
     * @param writeValues	The value to write to the device
     * @param numBits	The number of bits to write/read
     */
    public long[] transfer(long[] writeValues, int[] numBits) {
        if (writeValues.length != numBits.length) {
            throw new BadSPIConfigException("The number of values to write does not match array of data lengths");
        }
        for (int i = 0; i < numBits.length; ++i) {
            if (numBits[i] < 1 || numBits[i] > 32) {
                throw new BadSPIConfigException("All values in the data length must be >0 and <=32");
            }
        }
        long[] readValues = new long[writeValues.length];
        synchronized (semaphore) {
            tSPI.writeConfig_MSBfirst(bitOrder);
            tSPI.writeConfig_ClockHalfPeriodDelay(clockHalfPeriodDelay);
            tSPI.writeConfig_ClockPolarity(clockPolarity);
            tSPI.writeConfig_DataOnFalling(dataOnTrailing);
          
            tSPI.writeConfig_FramePolarity(!csActiveHigh);
            //Set up FPGA for chip select
			writeFrameMode(frameMode);
            tSPI.writeChannels_SS_Module(cs.getModuleForRouting());
            tSPI.writeChannels_SS_Channel(cs.getChannelForRouting());

            for (int i = 0; i < writeValues.length; ++i) {
                tSPI.writeConfig_BusBitWidth(numBits[i]);
                readValues[i] = trasferStatic(writeValues[i]);
            }
        }
        return readValues;
    }

    /**
     * Sets the bit order of the transfer sent and received values.
     * The value transfered/received will always be the lowest bits 
     * of the value. This method just sets the order in which those 
     * bits are transfered.
     *
     * @param bitOrder true=Most significant bit first, false=Least significant
     * bit first
     *
     * @see #BIT_ORDER_MSB_FIRST
     * @see #BIT_ORDER_LSB_FIRST
     */
    public final void setBitOrder(boolean bitOrder) {
        this.bitOrder = bitOrder;
    }

    /**
     * Sets the polarity of the clock when transferring data to the device
     *
     * @param clockPolarity true=Clock active low, false=Clock active high
     *
     * @see #CLOCK_POLARITY_ACTIVE_HIGH
     * @see #CLOCK_POLARITY_ACTIVE_LOW
     */
    public final void setClockPolarity(boolean clockPolarity) {
        this.clockPolarity = clockPolarity;
    }

    /**
     * If Data is valid at the beginning of the clock pulse or the end of the
     * clock pulse
     *
     * @param dataOnTrailing true=Process data on the trailing edge of the clock,
     * false=Process data on leading edge of the clock
     *
     * @see #DATA_ON_LEADING_EDGE
     * @see #DATA_ON_TRAILING_EDGE
     */
    public final void setDataOnTrailing(boolean dataOnTrailing) {
        this.dataOnTrailing = dataOnTrailing;
    }
	
    /**
     * Sets the Frame Mode which specifies the behavior of the chip select line in relation to 
     * the duration of the frame. 
     * 
     * @param frameMode 0 = low for duration of frame, 1 = pulse before transfer, 
     * 2 = pulse after transfer, 3 = pulse before and after transfer
     * 
     * @see #FRAME_MODE_CHIP_SELECT
     * @see #FRAME_MODE_PRE_LATCH
     * @see #FRAME_MODE_POST_LATCH
     * @see #FRAME_MODE_PRE_POST_LATCH_PULSE
     */
    public final void setFrameMode(char frameMode)
    {
        this.frameMode = frameMode;
    }

    /**
     * Set the frequence of the clock when sending data
     *
     * @param hz The frequency of the clock in hz
     *
     * @see #MIN_CLOCK_FREQUENCY
     * @see #MAX_CLOCK_FREQUENCY
     */
    public final void setClockRate(double hz) {
        
        double v = (1.0 / hz) / (2 * tDIO.readLoopTiming() / (kSystemClockTicksPerMicrosecond * 1e6));
        if (v < 1) {
            throw new BadSPIConfigException("Clock Rate too high. Hz: " + hz);
        }
        int delay = (int) (v + .5);
        if (delay > 255) {
            throw new BadSPIConfigException("Clock Rate too low. Hz: " + hz);
        }

        clockHalfPeriodDelay = delay;
    }

    /**
     * Set the frame mode of the SPI bus
     * 
     * @param frameMode The frame mode to set
     * 
     * @see #setFrameMode(char)
     */
    private void writeFrameMode(char frameMode)
    {
        switch (frameMode)
        {
            case 0: default:
                tSPI.writeConfig_LatchFirst(false);
                tSPI.writeConfig_LatchLast(false);
                break;
            case 1:
                tSPI.writeConfig_LatchFirst(true);
                tSPI.writeConfig_LatchLast(false);
                break;
            case 2:
                tSPI.writeConfig_LatchFirst(false);
                tSPI.writeConfig_LatchLast(true);
                break;
            case 3:
                tSPI.writeConfig_LatchFirst(true);
                tSPI.writeConfig_LatchLast(true);
                break;                
        }
    }
    public static class BadSPIConfigException extends RuntimeException {

        public BadSPIConfigException(String message) {
            super(message);
        }
    }
}
