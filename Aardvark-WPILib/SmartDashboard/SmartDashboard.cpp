/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "SmartDashboard/SmartDashboard.h"

#include "NetworkCommunication/UsageReporting.h"
#include "NamedSendable.h"
#include "WPIErrors.h"
#include "networktables/NetworkTable.h"

ITable* SmartDashboard::m_table = NULL;
std::map<ITable *, Sendable *> SmartDashboard::m_tablesToData;

void SmartDashboard::init(){
	m_table = NetworkTable::GetTable("SmartDashboard");
}

//TODO usage reporting

/**
 * Maps the specified key to the specified value in this table.
 * The key can not be NULL.
 * The value can be retrieved by calling the get method with a key that is equal to the original key.
 * @param keyName the key
 * @param value the value
 */
void SmartDashboard::PutData(std::string key, Sendable *data)
{
	if (data == NULL)
	{
		//TODO wpi_setWPIErrorWithContext(NullParameter, "value");
		return;
	}
    ITable* dataTable = m_table->GetSubTable(key);
    dataTable->PutString("~TYPE~", data->GetSmartDashboardType());
    data->InitTable(dataTable);
    m_tablesToData[dataTable] = data;
}

/**
 * Maps the specified key (where the key is the name of the {@link SmartDashboardNamedData}
 * to the specified value in this table.
 * The value can be retrieved by calling the get method with a key that is equal to the original key.
 * @param value the value
 */
void SmartDashboard::PutData(NamedSendable *value)
{
	if (value == NULL)
	{
		//TODO wpi_setWPIErrorWithContext(NullParameter, "value");
		return;
	}
	PutData(value->GetName(), value);
}

/**
 * Returns the value at the specified key.
 * @param keyName the key
 * @return the value
 */
//TODO Sendable *SmartDashboard::GetData(std::string key)
/*{
	ITable* subtable = m_table->GetSubTable(keyName);
	Sendable *data = m_tablesToData[subtable];
	if (data == NULL)
	{
		wpi_setWPIErrorWithContext(SmartDashboardMissingKey, keyName);
		return NULL;
	}
    return data;
}*/

/**
 * Maps the specified key to the specified complex value (such as an array) in this table.
 * The key can not be NULL.
 * The value can be retrieved by calling the RetrieveValue method with a key that is equal to the original key.
 * @param keyName the key
 * @param value the value
 */
void SmartDashboard::PutValue(std::string keyName, ComplexData& value)
{
	m_table->PutValue(keyName, value);
}

/**
 * Retrieves the complex value (such as an array) in this table into the complex data object
 * The key can not be NULL.
 * @param keyName the key
 * @param value the object to retrieve the value into
 */
void SmartDashboard::RetrieveValue(std::string keyName, ComplexData& value)
{
	m_table->RetrieveValue(keyName, value);
}

/**
 * Maps the specified key to the specified value in this table.
 * The key can not be NULL.
 * The value can be retrieved by calling the get method with a key that is equal to the original key.
 * @param keyName the key
 * @param value the value
 */
void SmartDashboard::PutBoolean(std::string keyName, bool value)
{
	m_table->PutBoolean(keyName, value);
}

/**
 * Returns the value at the specified key.
 * @param keyName the key
 * @return the value
 */
bool SmartDashboard::GetBoolean(std::string keyName)
{
	return m_table->GetBoolean(keyName);
}

/**
 * Maps the specified key to the specified value in this table.
 * The key can not be NULL.
 * The value can be retrieved by calling the get method with a key that is equal to the original key.
 * @param keyName the key
 * @param value the value
 */
void SmartDashboard::PutNumber(std::string keyName, double value){
	m_table->PutNumber(keyName, value);
}

/**
 * Returns the value at the specified key.
 * @param keyName the key
 * @return the value
 */
double SmartDashboard::GetNumber(std::string keyName)
{
	return m_table->GetNumber(keyName);
}

/**
 * Maps the specified key to the specified value in this table.
 * Neither the key nor the value can be NULL.
 * The value can be retrieved by calling the get method with a key that is equal to the original key.
 * @param keyName the key
 * @param value the value
 */
void SmartDashboard::PutString(std::string keyName, std::string value)
{
	m_table->PutString(keyName, value);
}

/**
 * Returns the value at the specified key.
 * @param keyName the key
 * @param value the buffer to fill with the value
 * @param valueLen the size of the buffer pointed to by value
 * @return the length of the string
 */
int SmartDashboard::GetString(std::string keyName, char *outBuffer, unsigned int bufferLen){
	std::string value = m_table->GetString(keyName);
	unsigned int i;
	for(i = 0; i<bufferLen-1&&i<value.length(); ++i)
		outBuffer[i] = (char)value.at(i);
	outBuffer[i] = '\0';
	return i;
}


/**
 * Returns the value at the specified key.
 * @param keyName the key
 * @return the value
 */
std::string SmartDashboard::GetString(std::string keyName)
{
	return m_table->GetString(keyName);
}
