#!/bin/bash

JSON_FILE="sounds-mp3.json"

if [ ! -f "$JSON_FILE" ]; then
echo "Error: File '$JSON_FILE' not found."
exit 1
fi

# Parse JSON and iterate over key-value pairs
jq -r 'to_entries[] | "\(.key) \(.value)"' "$JSON_FILE" | while read -r key value; do
    OUTPUT_FILE="sound_${key}.mp3"
    echo "$value" | base64 --decode > "$OUTPUT_FILE"
echo "Saved: $OUTPUT_FILE"
done
