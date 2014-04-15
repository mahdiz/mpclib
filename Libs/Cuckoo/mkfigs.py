#!/usr/bin/env python
'''
Created on Aug 23, 2012

@author: jkarlin
'''
from pylab import *
import sys

class Datum(object):
    def __init__(self, n, bad, k, g, group):
        self.n = n
        self.bad = bad
        self.k = k
        self.g = g
        self.group = group
    def __str__(self):
        return "n = %f, bad = %f, k = %d, g = %d, group = %f" % (self.n, self.bad, self.k, self.g, self.group)
        
def parse_file(filename):
    data = []
        
    
    
    for line in file(filename).readlines():
        line = line.strip()
        spl = line.split(',')
        n = float(spl[0].split('=')[1])
        e = float(spl[1].split('=')[1].split(' ')[0])
        bad = float(spl[1].split('=')[1].split(' ')[0])
        k = int(spl[2].split('=')[1])
        g = int(spl[3].split('=')[1])
        group = float(spl[4].split('=')[1])
        data.append((n, e, k, g, group))
    
    return data

def main():
    data = parse_file(sys.argv[1])
    #print data
    # Organize be e
    eness = {}
    for row in data:
        e = row[1]
        eness.setdefault(e, [])
        eness[e].append(row)
    
    # Sort each by network size
    for es in eness.itervalues():
        es.sort(lambda a,b: a[0] < b[0])

    
    # Plot each one with net size on x and group size on y
    figure(0)
    
    keys = eness.keys()
    keys.sort()
    

    for e in keys:        
        netsize = [row[0] for row in eness[e]]
        groupsize = [row[4] for row in eness[e]]
        loglog(netsize, groupsize, label='e=' + str(e), marker='o', linestyle='solid')
    #    handles, labels = ax.get_legend_handles_labels()
    # reverse the order
    #ax.legend(handles[::-1], labels[::-1])
    xlabel("Network Size")
    ylabel("Minimum Quorum Size")
    legend(loc=1)
    savefig(sys.argv[1] + '.pdf', format='pdf')
    show()
    
    # What do we want to plot?
    
if __name__ == '__main__':
    main()