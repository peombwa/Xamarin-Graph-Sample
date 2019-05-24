# Xamarin Graph Sample
This sample uses Microsoft.Graph .NET SDK to download OneDrive files in parallel in a Xamarin.Android app.
In addition, the sample uses Microsoft Authentication Library (MSAL) v3 to handle authentication for users' work or school and personal accounts.  

## Prerequisite
- Visual Studio 2019.
- Xamarin for Visual Studio.
- Windows 10.
- Either a personal or work or school.

## Register and configure the app
1. Navigate to Azure Poral [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `Xamarin-Grpah-Sample`.
   - In the **Supported account types** section, select **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. In the list of pages for the app, select **Authentication**..
   - In the **Redirect URIs** | **Suggested Redirect URIs for public clients (mobile, desktop)** section, check **the option of the form msal&lt;clientId&gt;://auth**
1. Select **Save**.
1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **Files.Read.All**. Use the search box if necessary.
   - Select the **Add permissions** button

## Build and debug
1. Open the `Services\GraphService.cs` file inside the `Xamarin-Grpah-Sample-Android` project of the solution and set the client id you registered by making this the value of the `clientID` field.

2. Open the `Xamarin-Grpah-Sample-Android\Properties\AndroidManifest.xml` file. Locate this element: `<data android:scheme="msalENTER_YOUR_CLIENT_ID" android:host="auth" />`. Replace ENTER_YOUR_CLIENT_ID with the application id value that you got when you registered your app. Be sure to retain msal before the application id. The resulting string value should look like this: <data android:scheme="msal<application id>" android:host="auth" />.
   
3. Press F5 to build and debug. Run the solution and sign in with either your personal or work or school account.