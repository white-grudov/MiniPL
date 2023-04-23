var a : int := 1;
var b : int := 2;
var c : int := 0;

var n : int;
print "Enter n: ";
read n;

var i : int;
for i in 1..n do
    c := a * b;
    a := b;
    b := b + c;
end for;

print "Result: ";
print c;