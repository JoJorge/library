# the general Makefile

TARGET = 

CPPFLAGS = -O3 -Wall -std=c++11 -g
CFLAGS = -O3 -Wall
CINCULDE = -I 
JAVAPACKAGES = ""
RM = *.vvp *.vcd *.class *.o *.exe

%.o: %.cpp
	g++ $< -c $@ $(CPPFLAGS) $(INCULDE)
%.o: %.c
	gcc $< -c $@ $(CFLAGS) $(INCLUDE)
%: %.cpp
	g++ $< -o $@ $(CPPFLAGS) $(INCULDE)
%: %.c
	gcc $< -o $@ $(CFLAGS) $(INCLUDE)
%.class: %.java
	javac $< -classpath $(JAVAPACKAGES)
%: %.class
	java $@
%.vvp: %.v 
	iverilog -o $@ $^
%.vcd: %.vvp
	vvp $<
%: %.vcd
	gtkwave $<

all: $(TARGET)
clean:
	rm $(RM)

