// Workflow domain modelling experiments in F#

// Iteration 4: Express workflow using Computation Expressions

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

type WorkflowBuilder() =
    member x.Bind(m, f) = Option.bind f m 
    member x.Return(value) = Some value

let wf = new WorkflowBuilder()
    

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

let endState = wf {  
    let! req = request "Project Alpha" 1000000
    let! rev = review ["Martin"] DateTime.Now req
    let! app = approve "PHB" DateTime.Now rev
    return app
    }

(*
val endState : Option<ApprovedState> =
  Some {request = Some {name = "Project Alpha";
                        budget = 1000000;};
        review = Some {names = ["Martin"];
                       date = 22-02-2016 21:33:57;};
        approval = Some {name = "PHB";
                         date = 22-02-2016 21:33:57;};}
*)

    
// It is not possible to put the approve before the review

// this does not compile
(*
let notGood = wf {
    let! req = request "Project Fail" 100000
    let! app = approve "Fast Eddie" DateTime.Now req
    let! rev = review ["John Slow"] DateTime.Now app
    return rev
}
*)
