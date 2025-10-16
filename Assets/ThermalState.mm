// Assets/Plugins/iOS/ThermalState.mm
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" {
    // Thermal State (iOS 11.0+)
    int GetThermalState() {
        if (@available(iOS 11.0, *)) {
            NSProcessInfo *processInfo = [NSProcessInfo processInfo];
            return (int)[processInfo thermalState];
        }
        return -1;
    }
    
    // Примерная температура по thermal state
    float GetEstimatedTemperature() {
        int state = GetThermalState();
        switch (state) {
            case 0: return 35.0f; // Nominal
            case 1: return 55.0f; // Fair
            case 2: return 75.0f; // Serious  
            case 3: return 95.0f; // Critical
            default: return -1.0f;
        }
    }
}