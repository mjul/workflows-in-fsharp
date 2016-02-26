// Workflow domain modelling experiments in F#

// Iteration 3: Try to extract the Option pattern matching boiler plate
//
// Credits to Debasish Ghosh for the inspiration - see his article
// http://debasishg.blogspot.dk/2015/02/functional-patterns-in-domain-modeling.html

open System

// Assuming a workflow for an approval procedure:
//
// Normal flow: request -> review -> approve

type Request = {name: string; budget: int}
type Review = {names: seq<string>; date: DateTime}
type Approval = {name: string; date: DateTime}

// Phantom types (marker interfaces, really)
type WorkflowState<'s> = {request: Option<Request>; review: Option<Review>; approval: Option<Approval>}

type RequestedState = WorkflowState<Request>
type ReviewedState = WorkflowState<Review>
type ApprovedState = WorkflowState<Approval>

// Define a new operator for composing these that takes care of
// the Option monad pattern matching to connect "Some" to Some and None otherwise

let inline (|-->) (x:Option<'a>) (f:'a -> Option<'b>) : Option<'b> =
    match x with
    | Some(v) -> (f v)
    | None -> None

// Note: The Option monad has build-in bind so
// the pattern match can be rewritten as just `Option.bind f x`

// with this we can write the workflow without the pattern-matching

// request is the constructor for the workflow - no state required
let request name budget =
    Some {RequestedState.request=Some {name=name; budget=budget};
          review=None; approval=None}
    
let review reviewers date (state:RequestedState) : Option<ReviewedState> =
    Some {ReviewedState.review=Some {names=reviewers; date=date}
          request=state.request; approval=state.approval}

let approve approver date (state:ReviewedState) : Option<ApprovedState> =
    Some {ApprovedState.approval=Some {name=approver; date=date};
          request=state.request; review=state.review}
    

// It still composes in the correct order
let endState =
    request "Project Alpha" 1000000
    |--> review ["Martin"] DateTime.Now
    |--> approve "PHB" DateTime.Now

(*
val endState : Option<ApprovedState> =
  Some {request = Some {name = "Project Alpha";
                        budget = 1000000;};
        review = Some {names = ["Martin"];
                       date = 22-02-2016 21:33:57;};
        approval = Some {name = "PHB";
                         date = 22-02-2016 21:33:57;};}
*)

    
// And now it is not possible to put the approve
// step before the review

(*
// this does not compile
let notGood =
    request "Project Fail" 100000
    |--> approve "Fast Eddie" DateTime.Now
    |--> review ["John Slow"] DateTime.Now
*)
