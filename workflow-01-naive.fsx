// Workflow domain modelling experiments in F#

// Iteration 1: Naïve version

// Assuming a workflow for an approval procedure:
//
// Normal flow: request -> review -> approve

open System

type Request = {name: string; budget: int}
type Review = {names: seq<string>; date: DateTime}
type Approval = {name: string; date: DateTime}

type WorkflowState =
    | Requested of Request
    | Reviewed of Request * Review
    | Approved of Request * Review * Approval
    
// let's try modelling with functions from a state to the next one,
// using Option type, in case it fails
    
// request is the constructor for the workflow - no state required
let request name budget : Option<WorkflowState> =
    // No validation - we will get to this later
    // We lift it into the Option monad
    Some (Requested {name=name; budget=budget})
    
let review reviewers date state : Option<WorkflowState>=
    match state with
        | Some(Requested(request)) -> Some(Reviewed(request, {names=reviewers; date=date}))
        | _ -> None

let approve approver date state : Option<WorkflowState> =
    match state with
        | Some(Reviewed(rq, rvs)) -> Some(Approved(rq, rvs, {name=approver; date=date}))
        | _ -> None


// Now let's see if it composes

let endState =
    request "Project Alpha" 1000000
    |> review ["Martin"] DateTime.Now
    |> approve "PHB" DateTime.Now

(*
val endState : Option<WorkflowState> =
  Some
    (Approved
       ({name = "Project Alpha";
         budget = 1000000;},{names = ["Martin"];
                             date = 20-02-2016 19:29:14;},
        {name = "PHB";
         date = 20-02-2016 19:29:14;}))
*)
    
    
// But unfortunately we can also do things in the wrong order
// even though the Option monad and pattern matching will
// give us a None result

let notGood =
    request "Project Fail" 100000
    |> approve "Fast Eddie" DateTime.Now
    |> review ["John Slow"] DateTime.Now

(*
   val notGood : Option<WorkflowState> = None
*)

