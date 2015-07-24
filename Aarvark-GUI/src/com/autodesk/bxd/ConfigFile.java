/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.autodesk.bxd;

import java.io.File;
import java.io.IOException;
import java.io.PrintWriter;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.StringTokenizer;

/**
 *
 * @author t_hics
 */
public class ConfigFile {
    
   public static enum Delims {Version, Loopback, TeamNumber}; 
   
    public ConfigFile (){
        File f = new File("config.txt");
        if (f.exists() == false){
            System.out.println();
        }
    }
    
    public boolean writeLine(String line){
        try {
            PrintWriter writer = new PrintWriter("config.txt", "UTF-8");
            writer.println(line);
            writer.close();
            return(true);
        }catch(Exception e){
            e.getCause();
            return(false);
        }
    }
    
    static String readFile(String path, Charset encoding) 
    throws IOException 
    {
      byte[] encoded = Files.readAllBytes(Paths.get(path));
      return new String(encoded, encoding);
    }
    
    public String readLine(Delims delim){
        String refined = "Cannot find the variable you are searching for!";
        try {
            String content = readFile("config.txt", StandardCharsets.UTF_8);
                StringTokenizer tokens4days = new StringTokenizer(content, "\n");
                String[] tokens = content.split("\n");
                for (int j = 0; j < tokens4days.countTokens(); j++){
                    if(tokens[j].contains(returnDelims(delim))){
                        refined = tokens[j].substring(tokens[j].lastIndexOf(returnDelims(delim)), tokens[j].length());
                    }
                }
            System.out.println(refined);
            return(refined);
        }catch(Exception e){
            e.getCause();
            return(null);
        }
    }
    
    public String returnDelims(Delims delim){
        String delimeter = "";
        switch(delim){
            case Version:
                delimeter = "[Version]: ";
                break;
            case Loopback:
                delimeter = "[Loopback]: ";
                break;
            case TeamNumber:
                delimeter = "[TeamNumber]: ";
            default: 
                delimeter = "Can't find the specified delimeter";
                break;
        } 
        return delimeter;
    }
    
    public void setVerison(){
        writeLine(returnDelims(Delims.Version) + "0.3.2.1");
    }
    
    public void setLoopBack(String name){
        writeLine(returnDelims(Delims.Loopback) + name);
    }
    
    public void setTeamNumber(int number){
        writeLine(returnDelims(Delims.Loopback) + number);
    }
    
    public void getVersion(){
        readLine(Delims.Version);
    }
    public void getLoopBack(){
        readLine(Delims.Version);
    }
    public void getTeamNumber(){
        readLine(Delims.Version);
    }  
}
