# Next_User

I wrote this simple program in 2013 during my first year of system administration.  It was probably my second C# project and I would use the application for about the next year while I continued to do desktop support.  

It was used to change the user displayed during the Windows login screen for domain computers.  This program was not digitally signed and does a lot of highly suspicious things, like modifying the registry (oh no!).  On a deeper level it does need access to some pretty arcane windows configuration settings and will even try to reach out to Active Directory should it be on a domain.  That said, the program nowadays gets flagged as a virus and I don't have the interest in resolving this (sorry, but it's a 6 year old project at this point).   

My summary from 2013:
I often log into end user systems locally but I don't want my username displayed when I log out.   Users are accustomed to their usernames being the default and will try to log in half a dozen times before realizing they're trying to login with the wrong account.  This of course runs the risk of locking my admin account or my having to take a confused phone call.   This program avoids that scenario (it has the added benefit of making me feel like an unseen IT ninja). It's now part of my routine when I log out.  I keep it on a network share with a short path and it takes just seconds.   System Requirements:   Windows XP/Vista machines will need to have .NET 3.5, 7 &amp; 8  have it by default.   It requires being run with administrator rights as technically anything to do with the login screen is a system setting.   

Extra Info:   This was adapted from a very simple batch script I made to accomplish the same task.  The GUI program is more or less a fancy way to quickly change the same registry keys; its benefit is primarily the auto-populate function for speed.   Auto-populate also prevents any typos that could represent an embarrassing IT fail. A user coming to work seeing their login displayed as MYCOMPANY\janesmoth instead of MYCOMPANY\janesmith would likely think their computer had been hacked.   This is the original batch script if you prefer:            

@echo off 
cls  
set /p UserInput= Domain\User :  
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI" /v LastLoggedOnSAMUser /t REG_SZ /d %UserInput% /f 
reg add "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI" /v LastLoggedOnUser /t REG_SZ /d %UserInput% /f  
echo. pause             
exit
