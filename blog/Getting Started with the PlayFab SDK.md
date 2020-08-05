# Getting Started with the PlayFab SDK
> by Johannes Ebner – Technical Specialist, Global Black Belt Gaming Team


# Introduction
Azure PlayFab is a really great product to help you build out your backend without creating all the systems like Item Catalogs, Shops etc. all by yourself. But PlayFab really evolves all the time with the **feedback you provide** - either through [Forums](https://community.playfab.com/index.html), [Slack](https://api.playfab.com/slack), direct contact to the PlayFab team or via the Global Black Belts like myself. With such a broad product and Microsoft’s best effort to not break anything for you while products evolve, some complexities inevitably arise.

So when you start with PlayFab and the SDK, you might be looking at all these APIs and ask yourself why? Why are these separate things? Why is there an Admin, Server, Client API, but then also specialized ones for certain systems, like Profiles?

Evolving over time means, that you create new best practices, new concepts. For example, one of the newer additions in terms of APIs is the [Entity Programming Model](https://docs.microsoft.com/en-us/gaming/playfab/features/data/entities/), which is different to what most developers would characterize as a “traditional” web API. But it goes very much hand in hand with modern game engines like Unreal and Unity, which have an [Entity-Component-Systems (ECS)](https://en.wikipedia.org/wiki/Entity_component_system) at their heart. This means, that the PlayFab team needs to carefully think about how to integrate these new Systems and Features, while keeping complexity in check and not breaking things.

# What does that mean?
This constant evolution means, that APIs need to be 

Based on PlayFab’s desire to reach as many developers as possible and cope with the maintenance work required by steadily evolving APIs, PlayFab has opted to not hand-craft SDKSs, but auto-generate SDK for you to use in your projects.
However, an auto-generated SDK like PlayFab’s might at times appear a bit harder to use than your usual-hand crafted library.

## Goal


## Prerequisites

### Azure PlayFab access
And, of course, since this is a Tutorial on how to use [Azure PlayFab](https://playfab.com/), you need an existing Title on PlayFab as well. You could use the same Microsoft Account you used for signing up to Azure.

# Many APIs, one SDK
Azure PlayFab has added many features over time and has evolved APIs to best cater for these features, or for new best-practices based on experience with customers.
As of today, Azure PlayFab’s APIs follow three rather different paradigms:

## Paradigm one: “Perspectives API”
(Perspectives API is a term I made up 😉)

The first paradigm takes a different perspective than most API’s I have encountered: the perspective of the user of the API – like a Player, an Admin etc. This means, you will find the same functionality duplicated or slightly changed under a different API. These end-user-perspective APIs are:

* AdminAPI
* ClientAPI
* ServerAPI

An example of this duplication is, for instance, the `ConsumeItem` request. It exists in both Client and Server APIs, does exactly the same, and uses the same request body and headers: except the authentication headers.
The Client variant wants a `SessionTicket`, while the Server variant wants a `SecretKey`.
Let’s look at these three API perspectives for a moment:

### Client
As the name implies, this is the “client side”, meaning it should be called within the context of a specific user, a player. Requires an authenticated Session.

### Server
This is a very broad API, which basically covers all operations which are supposed to be called from the server-side (or an Azure Function, if you do not want to implement a full server application)

### Admin
The Admin API is somewhat in the middle between Client and Server. The operations provided by this API is what you would typically see in an Admin Panel for a game.

## Paradigm Two: “Services API”
The second is the more common separation: an API per system, e.g. localization, groups, insights.

## Paradigm Three: “Entities API”
And lastly, there is the “Entity Programming Model”, which allows you to “attach” or “read” objects from specific Entity Types, which map to built-in functionality in Azure PlayFab, like a Player.

# Conclusion
