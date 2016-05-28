namespace Tzix.Model

open System.Collections.Generic

[<AutoOpen>]
module MultiMapTypes =
  // Assert: Sets are always nonempty.
  type MultiMap<'k, 'v when 'k: comparison and 'v: comparison> =
    internal
    | MultiMap of Map<'k, Set<'v>>

module MultiMap =
  let empty: MultiMap<'k, 'v> = MultiMap Map.empty

  let isEmpty (MultiMap m): bool =
    m |> Map.isEmpty

  let add (k: 'k) (v: 'v) (MultiMap m): MultiMap<'k, 'v> =
    let s' =
      match m |> Map.tryFind k with
      | Some s    -> s |> Set.add v
      | None      -> Set.singleton v
    in
      m |> Map.add k s' |> MultiMap

  let singleton (k: 'k) (v: 'v): MultiMap<'k, 'v> =
    empty |> add k v

  let removeOne (k: 'k) (v: 'v) (MultiMap m): MultiMap<'k, 'v> =
    match m |> Map.tryFind k with
    | Some s ->
        let s' = s |> Set.remove v
        if s'.IsEmpty
        then m |> Map.remove k
        else m |> Map.add k s'
    | None ->
        m
    |> MultiMap

  let removeAll (k: 'k) (MultiMap m): MultiMap<'k, 'v> =
    m |> Map.remove k |> MultiMap

  let findAll (k: 'k) (MultiMap m): seq<'v> =
    match m |> Map.tryFind k with
    | Some s      -> s :> seq<'v>
    | None        -> Seq.empty

  let containsKey k (MultiMap m): bool =
    m |> Map.containsKey k

  let contains (v: 'v) (MultiMap m): bool =
    m |> Map.exists (fun _ s -> s.Contains(v))

  let containsKeyValue (k, v) (MultiMap m) =
    match m |> Map.tryFind k with
    | Some s      -> s.Contains(v)
    | None        -> false

  let toSeq (MultiMap m): seq<'k * 'v> =
    seq {
      for (KeyValue (k, s)) in m do
        for v in s -> (k, v)
    }

  let ofSeq (kvs: seq<'k * 'v>): MultiMap<'k, 'v> =
    kvs |> Seq.fold (fun m (k, v) -> m |> add k v) empty

  let toList self =
    self |> toSeq |> Seq.toList

  let toMap (MultiMap m): Map<'k, Set<'v>> = m
  let ofMap (m: Map<'k, Set<'v>>): MultiMap<'k, 'v> = MultiMap m
