print "Fibonacci series\n";
print "Enter num: ";
var n : int;
read n;
var i : int;
var prev : int := 0;
var curr : int := 1;
print prev;
print "\n";
print curr;
print "\n";
for i in 2..n do
    var next : int := prev + curr;
    print next;
    print "\n";
    prev := curr;
    curr := next;
end for;
