# User Service Event Sourcing Demo

This is a small demo application using ASP.NET Core which demonstrates
using the event sourcing pattern.

It implements a simple user service where users can be created,
identifiers (such as your passport number) can be added or removed, the name can be 
set and the user state can be transitioned from active to terminated.

For each change, a new event is added.
