module Schedule

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

type LocationUsage = { name: Resource; used: Allocation list }

type Usage = 
    { locations: LocationUsage list; ageGroups: Resource list }

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

// Given a time range, get the events that are scheduled
let getEventsAt (startTime: Time) (endTime: Time) (events: ScheduledEvent list): ScheduledEvent list =
    let doesEventOccurAtGivenTime (event: ScheduledEvent) =
        // Has the event started?
        (event.startTime < endTime)
        &&
        // Has the event not finished yet?
        (event.finishTime > startTime)
    List.filter doesEventOccurAtGivenTime events

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

    // Get resource usage at a range starting with the given time
    let usedResourcesAt (time: Time): Usage =
        let events = getEventsAt time (time + nextEvent.duration) alreadyScheduledEvents
        
        let accumulateUsages (usageAcc: Usage) (currEvent: ScheduledEvent): Usage = 
            // Try to find an existing usage of the given location in the list
            match List.tryFind (fun u -> u.name = currEvent.event.location) usageAcc.locations with
            | Some existingLocationUsage ->
            let otherUsages = List.filter (fun u -> u.name <> existingLocationUsage.name) usageAcc.locations
            // Create a new usage out of a combination of the existing usage and the current usage
            {
            locations = {name = existingLocationUsage.name; used = currEvent.allocated :: existingLocationUsage.used} :: otherUsages;
            ageGroups = currEvent.event.ageGroup :: usageAcc.ageGroups
            }
            // Add the new usage to the usages list and return that as the new accumulator
            | None -> {
            locations = {name = currEvent.event.location; used = [currEvent.allocated]} :: usageAcc.locations; 
            ageGroups = currEvent.event.ageGroup :: usageAcc.ageGroups
            }
        // Start accumulating on an empty usage
        List.fold accumulateUsages {locations = []; ageGroups = []} events

    // Similarly, when checking if the next event can start at some time t0, in theory we need to check that the age group and 
    // proposed location resource are available at all time t such that t0 <= t < t0 + duration.
    // However, for efficiency, we don't need to actually check at all of these distinct time points.
    // We only need to check at time points that are within that range and are also 
    // (either 0 or the finish time of some previously scheduled event).
    // We only need to check at those time points because they are the time points at which resource usage changes.
    // Between such time points the resource usage doesn't change, so provided we check at those time points, 
    // we can be sure that the required resources are actually available at all times within that range. 
    
    // Given a certain time range, try to find a free allocation
    let getAllocation (usage: Usage): Allocation option =
        // Check whether the location is in use at all
        match List.tryFind (fun l -> l.name = nextEvent.location) usage.locations with
        // If there's some usage of the event's location
        | Some relevantLocationUsage ->
        // Return whether any of the different resources are unused for the entire range
        List.tryFind (fun n -> not (List.contains n relevantLocationUsage.used)) [1 .. (resourceAvailable nextEvent.location)]
        // If the location is not in use at all, allocate #1
        | None -> Some 1
            
    let rec findEarliestTime (timeCandidates : Time list) = 
        match timeCandidates with
        // This indicates a problem with the model - as the final member of the time candidates list should always be acceptable
        | [] -> raise (System.ArgumentException "The provided list can't be empty")
        | (candidate :: rest) ->
        let usage = usedResourcesAt candidate
        // If the age group already has an event
        if (List.contains nextEvent.ageGroup usage.ageGroups) then findEarliestTime rest
        // Try to find an allocation
        else 
        match getAllocation usage with
        | Some allocation ->
        (candidate, allocation)
        | None -> findEarliestTime rest
    
    findEarliestTime possibleStartTimes

// We schedule events one by one. At any given state we have a set of events that have already been scheduled and
// some event that is next to be scheduled. We determine the earliest possible start time for that next event and 
// add it to the list of scheduled events.
let scheduleNext (alreadyScheduled: ScheduledEvent list) (nextEvent: Event): ScheduledEvent list =
    let (time, allocation) = earliestStart alreadyScheduled nextEvent 
    let nextScheduledEvent: ScheduledEvent = {
        event = nextEvent;
        startTime = time;
        finishTime = time + nextEvent.duration;
        allocated = allocation;
    }
    alreadyScheduled @ [nextScheduledEvent]

// Given a set of events to be scheduled, we schedule them one by one, with the order of scheduling determined by the specified order array.
// the first event to be scheduled will be events[order[0]], followed by events[order[1]], etc, until we have a completely scheduled set of events.
let schedule (events: Event array) (order: int array) : ScheduledEvent list =
    let scheduleEvent (alreadyScheduled: ScheduledEvent list) (currIndex: int): ScheduledEvent list = 
        scheduleNext alreadyScheduled events.[order.[currIndex]]
    List.fold scheduleEvent [] [0 .. (events.Length - 1)]

// Return a cost function that we rank the fitness of a particular event scheduling order    
let athleticsScheduleCost (events: Event array) =
    let fitnessFunction (order: int array) : double =
        // We first schedule the events using the specified event order and then determine how long it will take to conduct all events (i.e. the finish time of the latest finishing event).
        // Since for a fitness function normally produces a higher value for a better solution, we take the negative of the finish time.
        // For example, if the finish time of our latest event is 181 minutes, then our fitness function would return -181.0
        let scheduled = schedule events order
        // Get a list of all finish times
        let finishTimes = List.map (fun se -> se.finishTime) scheduled
        // Return the latest finish time * -1 as a double
        double (-1 * List.max finishTimes)
    fitnessFunction