// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "IInputDevice.h"
#if PLATFORM_ANDROID
#include "Android/AndroidInputInterface.h"
#include "Android/AndroidApplication.h"
#endif
/**
 * 
 */

class JOYSTICKPLUGIN_API GenericInputDevicePlugin
{
public:
	GenericInputDevicePlugin();
	~GenericInputDevicePlugin();
};

//AndroidInputInterface 를 상속 받고 joystickaxisevent를 overloading
//
class JOYSTICKPLUGIN_API FGenericInputDevice : public IInputDevice
{
public:
    FGenericInputDevice(const TSharedRef<FGenericApplicationMessageHandler>& InMessageHandler);
    ~FGenericInputDevice();


    //Tick the interface (e.g. check for new controllers) 
    virtual void Tick(float DeltaTime) override;

    //Poll for controller state and send events if needed 
    virtual void SendControllerEvents() override;

    //Set which MessageHandler will get the events from SendControllerEvents. 
    virtual void SetMessageHandler(const TSharedRef< FGenericApplicationMessageHandler >& InMessageHandler) override;

    //Exec handler to allow console commands to be passed through for debugging 
    virtual bool Exec(UWorld* InWorld, const TCHAR* Cmd, FOutputDevice& Ar) override;

    bool OnControllerAnalog(FGamepadKeyNames::Type KeyName, int32 ControllerId, float AnalogValue);

    //IForceFeedbackSystem pass through functions 
    virtual void SetChannelValue(int32 ControllerId, FForceFeedbackChannelType ChannelType, float Value) override;
    virtual void SetChannelValues(int32 ControllerId, const FForceFeedbackValues& values) override;

    bool bBindingKeys = false;

    void OnInputKeyGenericDevice(FInputKeyEventArgs EventArgs);
    
    void OnInputAxisGenericDevice(FViewport* InViewport, int32 ControllerId, FKey Key, float Delta, float DeltaTime, int32 NumSamples, bool bGamepad);

#if PLATFORM_ANDROID
    FAndroidGamepadDeviceMapping* pnidevice;
#endif
private:
    /* Message handler */
    TSharedRef<FGenericApplicationMessageHandler> MessageHandler;
};