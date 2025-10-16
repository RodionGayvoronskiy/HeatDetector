package com.carx.benchmark;

import android.util.Log;
import java.util.HashMap;
import java.util.Map;

public class ThermalReader {
    private static final String TAG = "ThermalReader";
    
    public static Map<String, Float> getTemperatureMap() {
        Map<String, Float> temps = new HashMap<>();
        
        String[] cpuPatterns = {"cpu", "CPU", "xpu", "acpu", "kcpu"};
        String[] gpuPatterns = {"gpu", "GPU", "adreno", "mali", "power", "g3d"};
        String[] socPatterns = {"soc", "SoC", "tsens", "battery", "skin"};
        
        try {
            float cpuTemp = findTemperature(cpuPatterns);
            temps.put("cpu", cpuTemp);
            
            float gpuTemp = findTemperature(gpuPatterns);
            temps.put("gpu", gpuTemp);
            
            float socTemp = findTemperature(socPatterns);
            temps.put("soc", socTemp);
            
        } catch (Exception e) {
            Log.e(TAG, "Temperature scan error", e);
        }
        
        return temps;
    }
    
    private static float findTemperature(String[] patterns) {
        try {
            for (int i = 0; i < 30; i++) { // Больше зон
                String typePath = String.format("/sys/class/thermal/thermal_zone%d/type", i);
                String type = readFile(typePath);
                
                if (type != null) {
                    // Проверяем все паттерны
                    for (String pattern : patterns) {
                        if (type.toLowerCase().contains(pattern.toLowerCase())) {
                            String tempPath = String.format("/sys/class/thermal/thermal_zone%d/temp", i);
                            String tempStr = readFile(tempPath);
                            
                            if (tempStr != null && !tempStr.trim().isEmpty()) {
                                float temp = Float.parseFloat(tempStr.trim()) / 1000f;
                                return temp;
                            }
                        }
                    }
                }
            }
            
            String adrenoTemp = readFile("/sys/class/kgsl/kgsl-3d0/temp");
            if (adrenoTemp != null) {
                float temp = Float.parseFloat(adrenoTemp) / 1000f;
                Log.d(TAG, "Adreno GPU: " + temp);
                return temp;
            }
            
            String exynosTemp = readFile("/sys/devices/virtual/thermal/thermal_zone0/temp");
            if (exynosTemp != null) {
                return Float.parseFloat(exynosTemp) / 1000f;
            }
            
        } catch (Exception e) {
            Log.e(TAG, "findTemperature error", e);
        }
        return -1f;
    }
    
    public static String getAllThermalZones() {
        StringBuilder sb = new StringBuilder();
        try {
            for (int i = 0; i < 20; i++) {
                String typePath = String.format("/sys/class/thermal/thermal_zone%d/type", i);
                String tempPath = String.format("/sys/class/thermal/thermal_zone%d/temp", i);
                
                String type = readFile(typePath);
                String temp = readFile(tempPath);
                
                sb.append(String.format("Zone %2d: type='%s' temp='%s' %s\n", 
                    i, type != null ? type : "NULL", 
                    temp != null ? temp : "NULL",
                    type != null && type.toLowerCase().contains("cpu") ? "[CPU]" : ""));
            }
            
            sb.append("\nAlternative paths:\n");
            sb.append("Adreno: ").append(readFile("/sys/class/kgsl/kgsl-3d0/temp")).append("\n");
            sb.append("Exynos: ").append(readFile("/sys/devices/virtual/thermal/thermal_zone0/temp")).append("\n");
            
        } catch (Exception e) {
            sb.append("Error: ").append(e.getMessage());
        }
        return sb.toString();
    }
    
    private static String readFile(String path) {
        try {
            String result = tryReadWithRuntime(path);
            if (result != null) return result;
            
            return tryReadWithFileReader(path);
        } catch (Exception e) {
            Log.w(TAG, "All read methods failed for " + path + ": " + e.getMessage());
            return null;
        }
    }
    
    private static String tryReadWithRuntime(String path) {
        try {
            Process process = Runtime.getRuntime().exec(new String[]{"cat", path});
            java.io.BufferedReader reader = new java.io.BufferedReader(
                new java.io.InputStreamReader(process.getInputStream()));
            String line = reader.readLine();
            process.waitFor();
            return line;
        } catch (Exception e) {
            return null;
        }
    }
    
    private static String tryReadWithFileReader(String path) {
        try (java.io.BufferedReader reader = new java.io.BufferedReader(new java.io.FileReader(path))) {
            return reader.readLine();
        } catch (Exception e) {
            return null;
        }
    }
}