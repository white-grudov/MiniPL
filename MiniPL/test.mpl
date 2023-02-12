var n : int := 10;
var i : int;
var prev : int := 0;
var curr : int := 1;
print prev;
print curr;
for i in 2..n do
    var next : int := prev + curr;
    print next;
    prev := curr;
    curr := next;
end for;
