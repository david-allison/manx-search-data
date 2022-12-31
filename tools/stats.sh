# Counts the number of documents over time
# usage: tools/stats.sh > stats.csv
# macOS only - due to `date` command syntax
d=`date +%Y-%m-%d`
while [ "$d" != 2021-04-16 ]; do 
  d=$(date -j -v -1d -f "%Y-%m-%d" $d +%Y-%m-%d)

  commit=`git rev-list -n 1 --first-parent --before="$d 23:59" master`
  if [[ "$commit" == '' ]]
  then
      break
  fi
  git checkout $commit >/dev/null 2>&1
  lc=`find OpenData -name "document.csv" | wc -l`
  echo "$d,$lc"
done

git checkout -f master >/dev/null 2>&1