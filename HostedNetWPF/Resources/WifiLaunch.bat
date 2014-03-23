@echo off
REM Stopping shared connection
netsh wlan stop hostednetwork

@echo off
REM Configuring shared connection
netsh wlan set hostednetwork mode=allow ssid=%2 key=%4

@echo off
REM Starting shared connection
netsh wlan start hostednetwork