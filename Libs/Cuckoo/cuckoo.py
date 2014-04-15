#!/usr/bin/env pypy
import random
import math
import sys
import copy
from rangebst import RangeBST

from optparse import OptionParser
    
class Node(object):
    """ A node in the ring """
    def __init__(self, byzantine=False, pos=None):
        # Node position
        self.pos = pos 
        
        # Whether or not the node is byzantine
        self.byzantine = byzantine
        
    def __str__(self):
        print "%f %b" % (self.pos, self.byzantine)
        
        

class CuckooNetwork(object):
    def __init__(self, k, gamma):
        random.seed()
        # RangeBST lets you efficiently lookup nodes in a range of positions
        self.nodes = RangeBST()
        self.k = k
        self.gamma = gamma
        self.two_above_array = [1.0 / pow(2, i) for i in range(400)]

        self.quorums = {}
        self.log = 0
        self.redo_quorums()
        
    def clone(self):
        c =  CuckooNetwork(self.k, self.gamma)
        c.nodes = copy.deepcopy(self.nodes)
        c.quorums = self.quorums
        c.log = self.log
        return c
        
        
    def random_pos(self):
        return random.random()

    def __len__(self):
        return len(self.nodes)
    

    def two_above(self, val):        
        if val >= 1.0:
            return 1.0
        # We've cached the 1/powers of 2 values in self.two_above_array
        for (r, aval) in enumerate(self.two_above_array):
            if val > aval:
                return self.two_above_array[r-1]
            elif val == aval:
                return aval
            
        
        raise Exception("Unknown above")
    
    def k_region_nodes(self, pos):
        (start, end) = self.k_region(pos)
        return self.nodes.range(start, end)

    
    def k_region(self, pos):
        kr_size = self.k_region_size()
        reg = math.floor(pos / kr_size)
        return (kr_size*reg, kr_size*(reg+1))
    
    def k_region_size(self):
        if len(self) == 0:
            return 1.0
        # Assume network knows log estimate of size
        # and not actual size
        n = float(pow(2,self.log))
        
        # Get the size of a k-region
        return self.two_above(self.k/n)
    
    def remove(self, node):
        self.remove_from_quorums(node)
        self.nodes.remove(node.pos)
        
        # Did we just shrink the network by a log size?
        # If so, it's time to adjust quorum size
        if len(self) == 0:
            self.quorums = {}
        elif int(math.log(len(self), 2)) < self.log:
            self.log = int(math.log(len(self), 2))            
            self.redo_quorums() 
            

    def remove_from_quorums(self, node):
        reg = self.quorum_region(node.pos)
        self.quorums[reg].remove(node)
    
    def add_to_quorums(self, node):
        reg = self.quorum_region(node.pos)
        self.quorums[reg].add(node)
        
    def bootstrap_join(self, node):
        node.pos = self.random_pos()
        self.nodes.add(node.pos, node)
        self.add_to_quorums(node)
        # Did we just grow the network by a log size?
        # If so, it's time to adjust quorum size        
        if math.floor(math.log(len(self), 2)) > self.log:
            self.log = math.floor(math.log(len(self), 2))
            self.redo_quorums()         
                
    def join(self, node, update_quorums=False):       
        # Pick a random location
        rand = self.random_pos()
        evict = self.k_region_nodes(rand)

        # Reposition the other nodes in the k-region
        for evicted in evict:
            self.remove_from_quorums(evicted)
            self.nodes.remove(evicted.pos)
            
            # Secondary join (no evicting)
            evicted.pos = self.random_pos()            
            self.nodes.add(evicted.pos, evicted)
            self.add_to_quorums(evicted)

            
        # Add the new node
        node.pos = rand
        self.add_to_quorums(node)
        self.nodes.add(node.pos, node)

        # Did we just grow the network by a log size?
        # If so, it's time to adjust quorum size        
        if math.floor(math.log(len(self), 2)) > self.log:
            self.log = math.floor(math.log(len(self), 2))
            self.redo_quorums() 

    def quorum_size(self, pos):
        reg = self.quorum_region(pos)
        return len(self.quorums[reg])
    
    def avg_quorum_size(self):
        sum = 0.0
        for q in self.quorums.itervalues():
            sum += len(q)
        return sum / len(self.quorums)

    def quorum_region_size(self):
        # gamma*log(n) k_regions
        # where gamma is a small integer
        if self.log == 0:
            return 1.0
        
        # Get the size of a k-region
        g = float(self.gamma)
        
        
        return self.two_above(self.k_region_size() * (g * self.log))
        

    
    def quorum_region(self, pos):
        q_size = self.quorum_region_size()
        reg = math.floor(pos / q_size)
        return (q_size*reg, q_size*(reg+1))

    def redo_quorums(self):
        self.quorums = {}
        q_size = self.quorum_region_size()
        cur = 0.0
        while cur < 1.0:
            self.quorums[(cur,cur+q_size)] = set()
            cur += q_size
        

        for node in self.nodes:
            r = self.quorum_region(node.pos)
            self.quorums[r].add(node)
            
                    
    def verify(self):
        
        # Verify each quorum region has no more than 1/4 byzantine
        for quorum in self.quorums.itervalues():
            bad = len([1 for n in quorum if n.byzantine])
            
            if float(bad)/len(quorum) > 0.25:
                return False
        return True

    def byzantine_from_least_faulty_quorum(self):           
        faultiness = {}
        for (r, quorum) in self.quorums.iteritems():
            bad = len([1 for n in quorum if n.byzantine])            
            faultiness[r] = float(bad) / len(quorum)

        # Find the least faulty
        least = (1.1, None)
        for (r, fault) in faultiness.iteritems():
            if fault < least[0] and fault > 0.0:
                least = (fault, r)
        
        least_q = self.quorums[least[1]]
        # return the first byzantine node in this quorum
        for node in least_q:
            if node.byzantine:
                return node
            
        raise Exception("No node found")






class CommensalNetwwork(CuckooNetwork):

    def okay_join(self, pos):
        """ Here we need to see if there have been
        at least k secondary joins since the last 
        primary join """
        
        pass
        
            
    def random_quorum_select(self, pos, num_cuckoo):
        reg = self.quorum_region(pos)
        nodes = self.quorums[reg]
        return random.sample(nodes, num_cuckoo)
        
    def join(self, node):
        while True:
            rand = self.random_pos()
            if not self.okay_join(rand):
                continue
            
            g = self.avg_quorum_size()
            gprime = self.quorum_size(rand)
            num_cuckoo = self.k * gprime / g
            
            evict = self.random_quorum_select(rand, num_cuckoo)

            for evicted in evict:
                self.remove_from_quorums(evicted)
                self.nodes.remove(evicted.pos)
                
                # Secondary join (no evicting)
                evicted.pos = self.random_pos()            
                self.add_to_quorums(evicted)
                self.nodes.add(evicted.pos, evicted)

            # Add the new node
            node.pos = rand           
            self.add_to_quorums(node)
            self.nodes.add(node.pos, node)
            
            # Did we just grow the network by a log size?
            # If so, it's time to adjust quorum size        
            if math.floor(math.log(len(self), 2)) > self.log:
                self.log = math.floor(math.log(len(self), 2))
                self.redo_quorums() 

            break




def make_net_bootstrap(n,e,k,gamma):
    """ Creates a random network but does not
    use the cuckoo rule for each join since it's assumed
    none of the nodes are acting byzantine yet """
    net = CuckooNetwork(k=k, gamma=gamma)
    good = int(n/(1+e))
    bad = n - good
    

    for i in range(good):
        net.bootstrap_join(Node())
    
    for i in range(bad):
        net.bootstrap_join(Node(byzantine=True))

    return net



def make_net(n, e, k, gamma):
    """ Creates a Cuckoo'd Network from scratch
    where each join follows the Cuckoo rule """
    
    net = CuckooNetwork(k=k, gamma=gamma)
    good = int(n/(1+e))
    bad = n - good
    
    
    for i in range(good):
        net.join(Node())
    
    for i in range(bad):
        net.join(Node(byzantine=True))

    return net


def attack_net(net, num_attacks):
    for i in range(num_attacks):
        
        if i % (num_attacks / 10) == 0:
            stars = i / (num_attacks/10)
            sys.stderr.write('\r[' + '*'* stars + ' ' * (9-stars) + ']')
            sys.stderr.flush()        
        node = net.byzantine_from_least_faulty_quorum()
        net.remove(node)
        net.join(node)
        if not net.verify():
            return False
    return True
    
    
def do_experiment(opts):    
    for n in range(8, 22):
        # !!! VERY IMPORTANT !!!
        # Do not make network sizes powers of 2!  This will cause
        # the attack to be very slow (computationally) since the attack
        # removes and adds a single node.  This single node is on a power of 2
        # and causes the quorums to be recalculated each time they come and go.
        # Set n to a power of 2 + 1 instead! 

        N = math.pow(2,n) + 1     
        N = int(N)
        
        for E in [0.001, 0.05, 0.15, 0.25]: 
            # Find a k such that
            # k > 3ek + 3
            K = None
            for k in range(1,100):
                if k > 3.0*E*k + 3.0:
                    K = k
                    break
            if not K:
                print "Could not find a suitable K for n=%d, e=%f" % (N, E)
                continue

            if opts.k != 0:
                K = opts.k
            
            # Now we have n, e, and k.  Let's try for different gammas
            last = None
            for g in range(1, 50):
                # Skip this round if the quorum size is the same as last time
                # Q: Why would they ever be the same for different gammas?
                # A: Because they round up to the nearest power of 2

                if last:
                    last_qs = last.quorum_region_size()
                    last.gamma += 1
                    cur_qs = last.quorum_region_size()
                    if last_qs == cur_qs:
                        continue                
            
                net = make_net_bootstrap(n=N,k=K,e=E,gamma=g)

                net.gamma = g
                net.redo_quorums()
                
                verified = attack_net(net, opts.attackrounds)

                bad = N - int(N/(1+E))                

                if verified:
                    sys.stderr.write("n=%f, e=%f (%f bad), k=%d, g=%d, group=%f, verified=%r\n" % \
                                     (N,E, 100.0*bad/N,K,g,net.avg_quorum_size(), verified))
                    sys.stdout.write(" n=%f, e=%f (%f bad), k=%d, g=%d, group=%f, verified=%r\n" % \
                                     (N,E, 100.0*bad/N,K,g,net.avg_quorum_size(), verified))
                    sys.stdout.flush()
                if verified:
                    # We found one, let's move to the next set of parameters
                    break
    
                last = net


def parse_args():
    parser = OptionParser()

    parser.add_option("-k", "--k", type="int", default=0,
                      help="Value of k, 0=min. recommended value")
    
    parser.add_option("-a", "--attackrounds", type="int", default=1000,
                      help="Number of attack rounds network must withstand")
    

    (options, _args) = parser.parse_args(sys.argv)
    return options
    

def main():    
    opts = parse_args()
    do_experiment(opts)
    
if __name__ == '__main__':
    main()
