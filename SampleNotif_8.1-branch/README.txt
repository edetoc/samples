Steps

0) 
a channel is created in RegisterForPN()
        
		_channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();


1)
Associate the app with the store (this is mandatory so that push notifications can be pushed )
here we have reserved the name 'Notifications sample for 8.1'

2)
Go to Dashboard, App details , Services, Push notifications
click on 'Live Services site' hyperlink
write down the Package SID and client secret

Package SID:
ms-app://s-1-15-2-2360336843-1284140783-2046701058-1199869953-3317504286-2842920214-2729116323

secret:
3PYlRtSRXU85CTyD8Rhfw6xaOxMi3ebQ

3) 
Go to http://pushtestserver.azurewebsites.net/wns/, provide Package SID and client secret as well as the channel URI obtained in step 0


