#!/bin/bash

# Define the mapping
declare -A mapping=(
  ["Headphones"]=0
  ["Read"]=1
  ["Checkmark"]=2
  ["TrashTornado"]=3
  ["GestureDown"]=4
  ["GestureLeft"]=5
  ["GestureRight"]=6
  ["GestureUp"]=7
  ["WindChimes"]=8
  ["TapScreen"]=9
  ["Atomic"]=10
  ["Leave"]=11
  ["Arrive"]=12
  ["Idle"]=13
  ["ScratchHead"]=14
  ["RopePile"]=15
  ["Snooze"]=16
  ["Print"]=17
  ["Shovel"]=18
  ["Box"]=19
  ["Searching"]=20
  ["PaperAirplane"]=21
  ["Exclamation"]=22
  ["Writing"]=23
)

# Function to determine the type based on a key
get_type() {
  local key="$1"
  for name in "${!mapping[@]}"; do
    if [[ "$key" == *"$name"* ]]; then
      echo "${mapping[$name]}"
      return
    fi
  done
  echo 99 # Default type if no match found
}

data=$(cat raw.json | jq '
 .animations |= with_entries(
   .value.frames |= map(
     if .images != null and (.images | type == "array") then
       .images = (.images | add)
     else
       .images = []
     end
   )
 )
')


data=$(echo $data | jq -c '.animations | to_entries[]')

for entry in $data; do
  name=$(echo "$entry" | jq -r '.key')  # Extract the key
  frames=$(echo "$entry" | jq '.value.frames') # Extract the value
  type=$(get_type "$name")              # Get the type based on the key
  jq -n --arg name "$name" --argjson frames "$frames" --argjson type "$type" '{name: $name, frames: $frames}'
done | jq -s
exit
