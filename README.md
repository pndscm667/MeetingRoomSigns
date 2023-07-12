# MeetingRoomSigns
Displays events for M365 meeting rooms on a monitor outside the room.  Web server and SQL server are inside organization, room mailbox is in Exchange Online

![alt text](https://github.com/pndscm667/MeetingRoomSigns/raw/master/Screenshot/sign.jpg "Conference Room Sign")

This is a quick razor pages website using ASP.NET 7, Microsoft Graph, C#, and a local SQL database for quote of the day.  We deployed the solution onto VMWare horizon kiosks mounted outside each meeting room.
When the kiosk boots up the login script examines the zero client name and if it is identified as a kiosk it reads a CSV that matches the kiosk to the meetingroom and opens the correct URL in Chrome Kiosk Mode.

## PowerShell Login Script:
```powershell
#If this machine is a conference room sign then open Chrome with the appropriate URL
$ZeroClientLocation = Get-ItemProperty -path "hkcu:\volatile environment" -Name "ViewClient_Machine_Name"
$ZeroClientLocation = $ZeroClientLocation.ViewClient_Machine_Name
$VMS = Import-Csv -path \\myserver\ConferenceRooms.csv
foreach($VM in $VMS)
{
  if($ZeroClientLocation -eq $vm.ZeroClient)
  {
    c:\Progra~2\Google\Chrome\Application\chrome.exe --kiosk $vm.URL --disable-infobars -no-default-browser-check
  }
}
```

## Sample CSV
```
ZeroClient,URL
ZZOLKPITG01,http://meetingrooms/?meetingroom=itconfrm@contoso.com&displayname=IT%20Conference%20Room
ZZOLKPSGN08,http://meetingrooms/?meetingroom=computerlab@contoso.com&displayname=Computer%20Lab
```

## URL
The room address and display name are included in the URL as parameters read by the web app.  Format is http://yourwebserver/?meetingroom=yourmeetingroom@organization&displayname=displaynamehere

## MS Graph
You must create a new enterprise application in MS Entra ID (Azure AD to most of us) and configure it to use client secret (just the way this one is set up, I suggest using certificate for production 
but client secret is less steps).  Make sure the application has Calendars.Read for all users and give it administrator consent.  Some guidance can be found here:
https://learn.microsoft.com/en-us/graph/migrate-azure-ad-graph-configure-permissions?tabs=http%2Cupdatepermissions-azureadgraph-powershell
Make a note of client secret, client ID, and Tenant ID.
I had to use microsoft.graph 4.11 as the newer version did not seem to acknowledge ICalendarCalendarViewCollectionPage.  I'll look into this soon.

## Deploy Project
The application should run on an internal IIS server with the current asp.net core server hosting bundle installed.  The app was built against .net core 7.  The only files that should need to be modified
are index.cshtml.cs and index.cshtml.  In the code file change the clientID, tenandID, and clientSecret in getUsersAsync()  If you are using the quote of the day then change out the SQL connection string
for one that will work with your server.  In the HTML page change the company name (or add a logo) and modify the copyright at the bottom of the page.

## Quote of the day
Totally optional piece that I was asked to include.  Quotes are retrieved from an on-prem SQL server.  There are 330 quotes and they rotate through each one in order.  I asked for 365 but you can't have
everything, can you?  .sql files to create DB, table, and stored procedure are in the "QuoteOfTheDay" folder.  Also a .csv with the 330 quotes.  Don't hold me responsible for the quotes.  I just had to
implement it, I didn't collect them!




