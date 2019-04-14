# Customer Support Virtual Service Desk
[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)

This is a chatbot project with a host one page application. It has two sections in it.
  - ChatbotCode - Bot project using Azure Bot Service and C#
  - ChatbotCustomeUI - A sigle page we application with basic html and css to host the chatbot.

# Features
  - Incident Creation
  - Get the status of existing Incident

The service now intigration library has more features like Live agent transfer, check request status, get knowledge articles. But in the chat bot only above mentioned two features has been linked.

Visit [ALICE - Your Virtual Agent](https://mycustomerserviceportal.azurewebsites.net/) for demo. This site has a default user i.e. me "Mainak Chatterjee". This name is being transfered to bot from the hosting portal.

Please find the below sample script to follow during demo.



## Incident Creation
- **Bot:** Welcome to Virtual Service Desk.
I’m Alice, your virtual assistant.
Please note, the conversation is logged for training purposes.
Do not enter sensitive or restricted data (such as passwords or confidential data).
Type ‘Hi’ to start…
- **Me:** Hi
- **Bot:** Hi Mainak Chatterjee ! How can I assist you today?
- **Me:** I would like to log a ticket
- **Bot:** Please provide a brief description of the issue encountered along with the product name. Eg: Outlook is extremely slow
- **Me:** outlook is not syncing with new emails
- **Bot:** What would you like to mark the urgency of the issue as: Low, Medium, High
- **Me:** medium
- **Bot:** When did you notice this issue?
- **Me:** yesterday evening
- **Bot:** Could you please specify the number of affected users like 1 for single user or 2 and above for multiple users?
- **Me:** 1
- **Bot:** If you would like me to add any additional comment to the ticket, please type in the entire content before hitting the Enter key. you could use Shift+Enter to move to the next line of your comment.
- **Me:** Need to resolve as soon as possible
- **Bot:** Thank you for the details shared. I’m getting a ticket logged for you right away.
- **Bot:** An incident has been logged for you with the details provided. A Service Desk Agent will reach out to you shortly to assist you further. This issue can be reference via the case number INC0010003
- **Bot:** Is there anything else that I can help you with ? Please Respond with only ‘Yes’ or ‘No’…
- **Me:** No thanks
- **Bot:** Thank you for contacting Global Service Desk.

## Get the Incident Status
- **Bot:** Welcome to Virtual Service Desk.
I’m Alice, your virtual assistant.
Please note, the conversation is logged for training purposes.
Do not enter sensitive or restricted data (such as passwords or confidential data).
Type ‘Hi’ to start…
- **Me:** Hi
- **Bot:** Hi Mainak Chatterjee ! How can I assist you today?
- **Me:** what is the status of my incident
- **Bot:** Sure! Please specify the ticket number to help me look up the details.
- **Me:** INC0010003
- **Bot:** The ticket details are as below :
The ticket is currently in 'New' state. Soon your incident will get assigned to corresponding team.
- **Bot:** Would you like to add any comment to the ticket ? Please Respond with only ‘Yes’ or ‘No’.
- **Me:** Yes
- **Bot:** Please type in the entire comment before hitting the Enter key. You could use Shift+Enter if you would like to include multiple lines to your comment.
- **Me:** When will my issue get resolved
- **Bot:** Successfully added the comments to your ticket.
- **Bot:** Is there anything else that I can help you with ? Please Respond with only ‘Yes’ or ‘No’…
- **Me:** No Thanks
- **Bot:** Thank you for contacting Global Service Desk.
