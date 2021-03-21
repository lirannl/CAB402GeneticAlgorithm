﻿module Schedule

// A resource might be either an age group (e.g. U16B) or a type of equipment needed for an event (e.g. a "track")
type Resource = string

// A point in time relative to the start of the competition or an event duration, both measured in whole minutes.
type Time = int

// If there are more than one pieces of equipment of the same type, we need to record which one will be allocated to a particular event.
type Allocation = int

// An athletics event made up of an event name (e.g. "100m Spring"), 
// and age group (.e.g. "U13G" = Under 13 girls), 
// the type of location resource required to conduct the event (e.g. a "track") 
// and the expected duration of the event measured in whole minutes.
type Event = { eventName: string; ageGroup: Resource; location: Resource; duration: Time }

// An event together with a decision as to when it should start relative to the start of the competition
// and which specific location has been allocated to it. 
// For example, if there are 4 separate sand "pits" available for long jump and triple jump events, 
// then this particular event might for example be allocated to pit 2.
// The finish time will always be start time plus the duration of the event.
type ScheduledEvent = { event: Event; startTime: Time; finishTime: Time; allocated: Allocation }

type Usage = { name: string; used: Allocation list; ageGroup: string }

// Given a list of comparable elements, determines which position an element would occupy in said list (if it were sorted)
let theoreticalPos (elem: 'a) (elemsList: 'a List): int =
    let sortedList = List.sort (elem :: elemsList)
    List.findIndex (fun sortedElem -> sortedElem = elem) sortedList

// How many resources of each type are available at the competition arena:
let resourceAvailable (resource: Resource) : int =
    match resource with
    | "scissors" -> 4 // four thin high jump mats are available that are suitable for younger age groups using Scissor technique
    | "flop" -> 2     // two thicker high jum mats are available for older age groups using Fosbury flop technique.
    | "pit" -> 4      // four sand pits are available for long jump and triple jump events
    | "shotput" -> 3  // three shotput circles are available
    | "discus" -> 3   // three larger caged circles are available for discus events
    | _ -> 1          // there is only one of all the other resources, e.g. only one track for running events, only one javelin area, etc.

// Given a time, get the events that are scheduled
let getEventsAt (time: Time) (events: ScheduledEvent list): ScheduledEvent list =
    let doesEventOccurAtGivenTime (event: ScheduledEvent) =
        // Has the event started already?
        (event.startTime < time)
        &&
        // Has the event not finished yet?
        (event.finishTime > time)
    List.filter doesEventOccurAtGivenTime events

// Get the resource usage of a specified event
let getUsage (event: ScheduledEvent): Usage =
    {
    ageGroup = event.event.ageGroup;
    name = event.event.location;
    // For each event, one usage would be counted
    used = [event.allocated];
    }

// Given a set of events that have already been scheduled, determine the earliest possible start time for the specified next event
// Also return the allocation of which specific instance of the location resource it will be allocated to.
// The allocated location resource and the age group must be available for the entire duration of the event
// (startTime <= t < startTime + event.duration) i.e. That age group cannot be simultaneous doing some other event 
// nor can that particular allocated resource be used by any other event during that period.
let earliestStart (alreadyScheduledEvents: ScheduledEvent list) (nextEvent:Event): (Time * Allocation) =
    // TODO: add correct implementation here 

    // In searching for the earliest possible start time, we don't need to consider every possible integer start time.
    // We know that the earliest possible start time will always be either 0 (i.e. the very start of the competition) 
    // or it will start immediately after some other already scheduled event (that were waiting on) finishes.
    // For example, if we have already scheduled:
    //    U15-17G Javelin at startTime 0 with finishTime 36
    //    U8B 70m Sprint at startTime 0 with finishTime 1
    //    U13-14B Long Jump at startTime 0 with finishTime 17
    //    U15-17G 700m Walk at startTime 36 with finishTime 41
    // then we know the earliest possible starting time of the next event is either 0, 1, 17, 36 or 41
    // This makes the algorithm much faster as we only need to consider up to 5 possible start times, 
    // rather than up to 41 if every possible start point needed to be considered.
    let possibleStartTimes: Time list = 
        let getFinish = fun (scheduled: ScheduledEvent) -> scheduled.finishTime
        List.sort ([0] @ (List.map getFinish alreadyScheduledEvents))

    // Get resource usage at a given time
    let usedResourcesAt (time: Time): Usage list =
        let events = getEventsAt time alreadyScheduledEvents
        
        let usages = List.map getUsage events
        let accumulateUsages (usageAcc: Usage list) (currUsage: Usage) = 
            // Try to find an existing usage of the given resource in the list
            let existingUsageOption = List.tryFind (fun u -> u.name = currUsage.name) usageAcc
            match existingUsageOption with
            | Some existingUsage ->
            let otherUsages = List.filter (fun u -> u.name <> existingUsage.name) usageAcc
            // Create a new usage out of a combination of the existing usage and the current usage to be added to the accumulator
            let updatedUsage: Usage = 
                {
                    ageGroup = existingUsage.ageGroup; 
                    name = existingUsage.name; 
                    used = existingUsage.used @ currUsage.used
                }
            (updatedUsage) :: otherUsages
            // Add the new usage to the usages list and return that as the new accumulator
            | None -> currUsage :: usageAcc
        List.fold accumulateUsages [] usages

    // Similarly, when checking if the next event can start at some time t0, in theory we need to check that the age group and 
    // proposed location resource are available at all time t such that t0 <= t < t0 + duration.
    // However, for efficiency, we don't need to actually check at all of these distinct time points.
    // We only need to check at time points that are within that range and are also 
    // (either 0 or the finish time of some previously scheduled event).
    // We only need to check at those time points because they are the time points at which resource usage changes.
    // Between such time points the resource usage doesn't change, so provided we check at those time points, 
    // we can be sure that the required resources are actually available at all times within that range. 

    // Check availability on all relevant points in "possibleStartTimes"
    let determineRelevantCandidates (startTime: Time) (endTime: Time): Time list =
        List.filter (fun time -> (time >= startTime) && (time <= endTime)) possibleStartTimes
    
    // Given a certain time range, determine whether the required resources will be available throughout
    // This function is meant to be called with None as the list, since it'll generate its own list
    let rec isTimeAcceptable (time: Time) (candidatesToCheckOption: Time list option): bool =
        match candidatesToCheckOption with
        | Some [] -> true // None of the candidates lack the necessary resources
        | Some (candidate :: rest) -> 
        (
            let usages = usedResourcesAt candidate
            // If the age group is unavailable
            if ((List.tryFind (fun u -> u.ageGroup = nextEvent.ageGroup) usages) <> None) then false
            else
            // Check whether the location is in use
            let relevantUsageOption = List.tryFind (fun u -> u.name = nextEvent.location) usages
            match relevantUsageOption with
            // If there's some usage of the event's location
            | Some relevantUsage ->
            let locationVacant = relevantUsage.used.Length < (resourceAvailable nextEvent.location)
            // Make sure the target agegroup isn't already scheduled for something
            // If all needed resources are available continue searching
            if (locationVacant) then isTimeAcceptable time (Some rest)
            else false
            // If the location is not in use at all
            | None -> isTimeAcceptable time (Some rest)
        )
        // Get a list of the startTimes that are within range
        | None -> isTimeAcceptable time (Some (determineRelevantCandidates time (time + nextEvent.duration)))
            
    let rec findEarliestTime (timeCandidates : Time list) = 
        match timeCandidates with
        | (candidate :: rest) ->
        if (isTimeAcceptable candidate None) then
            let usageOption = List.tryFind (fun u -> u.name = nextEvent.location) (usedResourcesAt candidate)
            match usageOption with
            | Some usage ->
            // Find the first unused room
            let allocation = List.find (fun n -> not (List.contains n usage.used)) [1 .. (resourceAvailable nextEvent.location)]
            (candidate, allocation)
            // If there's no usage, allocate location 1
            | None -> (candidate, 1)
        else findEarliestTime rest
        | [] -> raise (System.ArgumentException "The provided list can't be empty")
    
    findEarliestTime possibleStartTimes

// We schedule events one by one. At any given state we have a set of events that have already been scheduled and
// some event that is next to be scheduled. We determine the earliest possible start time for that next event and 
// add it to the list of scheduled events.
let scheduleNext (alreadyScheduled: ScheduledEvent list) (nextEvent: Event): ScheduledEvent list =
    // TODO: add correct implementation here
    raise (System.NotImplementedException "scheduleNext")

// Given a set of events to be scheduled, we schedule them one by one, with the order of scheduling determined by the specified order array.
// the first event to be scheduled will be events[order[0]], followed by events[order[1]], etc, until we have a completely scheduled set of events.
let schedule (events: Event array) (order: int array) : ScheduledEvent list =
    // TODO: add correct implementation here 
    raise (System.NotImplementedException "schedule")

// Return a cost function that we rank the fitness of a particular event scheduling order    
let athleticsScheduleCost (events: Event array) =
    let fitnessFunction (order: int array) : double =
        // We first schedule the events using the specified event order and then determine how long it will take to conduct all events (i.e. the finish time of the latest finishing event).
        // Since for a fitness function normally produces a higher value for a better solution, we take the negative of the finish time.
        // For example, if the finish time of our latest event is 181 minutes, then our fitness function would return -181.0
        // TODO: add correct implementation here 
        raise (System.NotImplementedException "fitnessFunction")
    fitnessFunction