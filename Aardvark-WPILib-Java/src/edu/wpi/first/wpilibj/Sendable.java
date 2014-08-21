package edu.wpi.first.wpilibj;

import edu.wpi.first.wpilibj.tables.ITable;


/**
 * The base interface for objects that can be sent over the network
 * through network tables.
 */
public interface Sendable {
    /**
     * Initializes a table for this sendable object.
     * @param subtable The table to put the values in.
     */
    public void initTable(ITable subtable);
    
    /**
     * @return the table that is currently associated with the sendable
     */
    public ITable getTable();

    /**
     * @return the string representation of the named data type that will be used by the smart dashboard for this sendable
     */
    public String getSmartDashboardType();
}
