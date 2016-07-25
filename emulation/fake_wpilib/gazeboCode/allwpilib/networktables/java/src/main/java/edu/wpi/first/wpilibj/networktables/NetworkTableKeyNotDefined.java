package edu.wpi.first.wpilibj.networktables;

import java.util.NoSuchElementException;

/**
 * An exception throw when the lookup a a key-value fails in a {@link NetworkTable}
 * 
 * @deprecated to provide backwards compatability for new api
 * 
 * @author Mitchell
 *
 */
public class NetworkTableKeyNotDefined extends NoSuchElementException {

	/**
	 * @param key the key that was not defined in the table
	 */
	public NetworkTableKeyNotDefined(String key) {
		super("Unkown Table Key: "+key);
	}

}
