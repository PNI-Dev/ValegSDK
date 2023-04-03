#pragma once


#include <InputDevice.h>

#include "IJoystickPlugin.h"
#include "GenericInputDevicePlugin.h"
#include "JoystickDevice.h"

DECLARE_LOG_CATEGORY_EXTERN(JoystickPluginLog, Log, All);

class FJoystickPlugin : public IJoystickPlugin
{

public:
	virtual TSharedPtr<class IInputDevice> CreateInputDevice(const TSharedRef<FGenericApplicationMessageHandler>& InMessageHandler) override
	{
#if PLATFORM_WINDOWS
		return JoystickDevice;
#else
		return MakeShareable(new FGenericInputDevice(InMessageHandler));
#endif
	}

	void ShutdownModule() override
	{
		IJoystickPlugin::ShutdownModule();
		JoystickDevice = nullptr;
	}

	void StartupModule() override;

	TSharedPtr< class FJoystickDevice > JoystickDevice;

	void *SDL2LibraryHandle;
};
