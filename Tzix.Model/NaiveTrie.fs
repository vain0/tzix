namespace Tzix.Model

[<AutoOpen>]
module NaiveTrieTypes =
  type NaiveTrie<'k, 'x when 'k: comparison> =
    | NaiveTrie of value: option<'x> * transferMap: Map<'k, NaiveTrie<'k, 'x>>

module NaiveTrie =
  let singleton (x: 'x): NaiveTrie<'k, 'x> =
    NaiveTrie (Some x, Map.empty)

  let map (f: 'x -> 'y): NaiveTrie<'k, 'x> -> NaiveTrie<'k, 'y> =
    let rec loop =
      function
      | NaiveTrie (x, m) ->
          (x |> Option.map f, m |> Map.map (fun _ -> loop)) |> NaiveTrie
    in loop

  let add (ks: list<'k>) (value: 'x): NaiveTrie<'k, 'x> -> NaiveTrie<'k, 'x> =
    let rec loop =
      function
      | ([], NaiveTrie (_, m)) ->
          NaiveTrie (Some value, m)
      | (k :: ks, NaiveTrie (x, m)) ->
          let child = 
            match m |> Map.tryFind k with
            | Some n      -> n
            | None        -> NaiveTrie (None, Map.empty)
          let child' =
            loop (ks, child)
          in NaiveTrie (x, m |> Map.add k child')
    fun self ->
      loop (ks, self)

  let sub (k: 'k) (NaiveTrie (x, m)) =
    m |> Map.tryFind k
