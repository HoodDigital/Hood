#!/bin/bash
set -uo pipefail  

# Record start time in seconds since epoch
START_TIME=$(date +%s)

# Ensure local dotnet tools are configured
if [[ ! -f "./.config/dotnet-tools.json" ]]; then
  echo "dotnet-tools.json not found. Are you in the right directory?"
  exit 1
fi

# Check dependencies
command -v dotnet >/dev/null 2>&1 || { echo >&2 "dotnet is not installed."; exit 1; }
dotnet tool list | grep -q csharpier || { echo >&2 "csharpier is not installed (local)."; exit 1; }
dotnet tool list | grep -q jb || { echo >&2 "JetBrains CLI (jb) is not installed (local)."; exit 1; }

# Run CSharpier and record its exit code
echo "Running CSharpier check..."
dotnet csharpier check .
CSHARPIER_EXIT=$?

# Run InspectCode and record its exit code
echo "Running JetBrains InspectCode..."
mkdir -p .jb
dotnet jb inspectcode \
  --caches-home="./.jb/caches" \
  -o="./.jb/report.xml" \
  "./Hood.sln" \
  --format="Xml"
JB_EXIT=$?

grep "<Issue " .jb/report.xml

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Print friendly success/failure messages
if [[ $CSHARPIER_EXIT -ne 0 ]]; then
  echo -e "${RED}CSharpier failed with exit code $CSHARPIER_EXIT.${NC}"
else
  echo -e "${GREEN}CSharpier succeeded.${NC}"
fi

if [[ $JB_EXIT -ne 0 ]]; then
  echo -e "${RED}JetBrains InspectCode failed with exit code $JB_EXIT.${NC}"
else
  echo -e "${GREEN}JetBrains InspectCode succeeded.${NC}"
fi

# Check if report.xml contains any issue entries
ISSUE_COUNT=$(grep "<Issue " .jb/report.xml | wc -l)
if [[ $ISSUE_COUNT -gt 0 ]]; then
  echo -e "${RED}JetBrains InspectCode reported $ISSUE_COUNT issue(s).${NC}"
  JB_EXIT=1
else
  echo -e "${GREEN}JetBrains InspectCode found no issues.${NC}"
fi

# Final combined result
if [[ $CSHARPIER_EXIT -ne 0 || $JB_EXIT -ne 0 ]]; then
  echo -e "${RED}One or more tools reported issues.${NC}"
  EXIT_CODE=1
else
  echo -e "${GREEN}All checks passed successfully.${NC}"
  EXIT_CODE=0
fi
# Record end time
END_TIME=$(date +%s)

# Calculate elapsed time in seconds
ELAPSED=$((END_TIME - START_TIME))

# Format elapsed time as H:MM:SS
# Calculate hours, minutes, seconds
hours=$((ELAPSED / 3600))
minutes=$(((ELAPSED % 3600) / 60))
seconds=$((ELAPSED % 60))

# Pad with leading zeros if needed
pad() {
  num=$1
  if (( num < 10 )); then
    echo "0$num"
  else
    echo "$num"
  fi
}

h=$(pad $hours)
m=$(pad $minutes)
s=$(pad $seconds)

echo "Total time elapsed: ${h}:${m}:${s}"

exit $EXIT_CODE