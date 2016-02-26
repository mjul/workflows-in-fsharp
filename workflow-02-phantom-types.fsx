// Workflow domain modelling experiments in F#

// Iteration 2: Try to use "phantom types" to statically
// enforce the order of the workflow
//
// Credits to Debasish Ghosh for the inspiration - see his article
// http://debasishg.blogspot.dk/2015/02/functional-patterns-in-domain-modeling.html

// Assuming a workflow for an approval procedure:
//
// Normal flow: request -> review -> approve

open System

type Request = {name: string; budget: int}
type Review = {names: seq<string>; date: DateTime}
type Approval = {name: string; date: DateTime}

// Phantom types (marker interfaces, really)
type WorkflowState<'s> = {request: Option<Request>; review: Option<Review>; approval: Option<Approval>}

type RequestedState = WorkflowState<Request>
type ReviewedState = WorkflowState<Review>
type ApprovedState = WorkflowState<Approval>


// request is the constructor for the workflow - no state required
let request name budget =
    Some {RequestedState.request=Some {name=name; budget=budget};
          review=None; approval=None}
    
let review reviewers date (state:Option<RequestedState>) : Option<ReviewedState> =
    match state with
        | Some(state) -> Some {ReviewedState.review=Some {names=reviewers; date=date}
                               request=state.request; approval=state.approval}
        | None -> None
    
let approve approver date (state:Option<ReviewedState>) : Option<ApprovedState> =
    match state with
        |Some(state) -> Some {ApprovedState.approval=Some {name=approver; date=date};
                              request=state.request; review=state.review}
        |None -> None


// It still composes in the correct order
let endState =
    request "Project Alpha" 1000000
    |> review ["Martin"] DateTime.Now
    |> approve "PHB" DateTime.Now

(*
val endState : Option<ApprovedState> =
  Some {request = Some {name = "Project Alpha";
                        budget = 1000000;};
        review = Some {names = ["Martin"];
                       date = 22-02-2016 20:46:23;};
        approval = Some {name = "PHB";
                         date = 22-02-2016 20:46:23;};}   
*)

    
// And now it is not possible to put the approve
// step before the review

(*
// this does not compile
let notGood =
    request "Project Fail" 100000
    |> approve "Fast Eddie" DateTime.Now
    |> review ["John Slow"] DateTime.Now
*)
