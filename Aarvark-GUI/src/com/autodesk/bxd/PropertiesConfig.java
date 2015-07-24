/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.autodesk.bxd;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Properties;

/**
 *
 * @author t_hics
 */
public class PropertiesConfig {
    public static Properties prop = new Properties();
    
    public PropertiesConfig(){
    }
    
    public void setProp(String property, String value){
        
        OutputStream output = null;
	try {
		output = new FileOutputStream("config.properties");
		// set the properties value
                    prop.setProperty(property, value);
		// save properties to project root folder
		prop.store(output, null);
	} catch (IOException io) {
		io.printStackTrace();
	} finally {
		if (output != null) {
			try {
				output.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
 
	}
    }
    
    public String readProp(String resource){
	InputStream input = null;
	try {
		input = new FileInputStream("config.properties");
		// load a properties file
		prop.load(input);
	} catch (IOException ex) {
		ex.printStackTrace();
	} finally {
		if (input != null) {
			try {
				input.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}
        // get the property value and print it out
        return prop.getProperty(resource);
    }
    
    public int getTeamNumber(){
	InputStream input = null;
	try {
		input = new FileInputStream("config.properties");
		// load a properties file
		prop.load(input);
	} catch (IOException ex) {
		ex.printStackTrace();
	} finally {
		if (input != null) {
			try {
				input.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}
        // get the property value and print it out
        return Integer.parseInt(prop.getProperty("TeamNumber"));
    }
}
