'''
Created on Aug 6, 2012

@author: jkarlin
'''

class TNode(object):
    def __init__(self, pos, data, left=None, right=None):
        self.pos = pos
        self.left = left
        self.right = right
        self.data = data
        
class RangeBST(object):
    def __init__(self):
        self.root = None
        self.count = 0
        
    def __len__(self):
        return self.count
    
    def _insert(self, cur, pos, data):
        if pos < cur.pos:
            if cur.left is None:
                cur.left = TNode(pos, data)
            else:
                self._insert(cur.left, pos, data)
                
        else:
            if cur.right is None:
                cur.right = TNode(pos, data)
            else:
                self._insert(cur.right, pos, data)
        
    def _cut_min(self, cur):
        if not cur.left:
            # this is as far left as we go
            if cur.right:
                return (cur, cur.right)
            else:
                return (cur, None)
            
        else:
            (min, cur.left) = self._cut_min(cur.left)
            return (min, cur)


    def _delete(self, cur, pos):
        if cur is None:
            raise Exception("Could not find node")
        
        
        if cur.pos > pos:
            cur.left = self._delete(cur.left, pos)
        elif cur.pos < pos:
            cur.right = self._delete(cur.right, pos)
        else: # ==
            if cur.left is None and cur.right is None:
                return None
            elif cur.left and cur.right is None:
                return cur.left
            elif cur.right and cur.left is None:
                return cur.right
            else: # Both are not None
                R = cur.right
                parent = cur
                while True:
                    if R.left is None:
                        break
                    parent = R                    
                    R = R.left
                if parent.right == R:
                    parent.right = R.right
                elif parent.left == R:
                    parent.left = R.right

                R.right = cur.right
                R.left = cur.left
                return R
        return cur

#    def _delete(self, cur, pos):
#        if cur is None:
#            raise Exception("Could not find node")
#        print cur.pos
#        if cur.pos == pos:
#            self.count -= 1
#            # This is the node to delete
#            if cur.left is None and cur.right is None:
#                return None
#            elif cur.left is None and cur.right:
#                return cur.right
#            elif cur.left and cur.right is None:
#                return cur.left
#            elif cur.left and cur.right:
#                (min, cur.right) = self._cut_min(cur.right)
#                min.left = cur.left
#                min.right = cur.right
#                return min
#                        
#        elif pos < cur.pos:
#            cur.left = self._delete(cur.left, pos)
#        else:
#            cur.right = self._delete(cur.right, pos)
#
#        return cur
            
    def add(self, pos, data):
        if self.root is None:
            self.root = TNode(pos, data)        
        else:
            self._insert(self.root, pos, data)
        self.count += 1
        
    def remove(self, pos):
        if self.root is None:
            raise Exception("No root found!")
        self.root = self._delete(self.root, pos)
        
        self.count -= 1
        
    def _find(self, cur, pos):
        if cur is None:
            raise Exception("Could not find object")
        
        if cur.pos == pos:
            return cur.data
        
        elif pos < cur.pos:
            return self._find(cur.left, pos)
        else:
            return self._find(cur.right, pos)

    def __iter__(self):
        stack = []
        node = self.root
        while stack or node:
            if node:
                stack.append(node)
                node = node.left
            else:
                node = stack.pop()
                yield node.data
                node = node.right
                

        
    def find(self, pos):
        return self._find(self.root, pos)
    
    def range(self, low, high):
        return self._range(self.root, low, high)
    
    def _range(self, cur, low, high):
        if cur is None:
            return []
        if low < cur.pos and high <= cur.pos:
            return self._range(cur.left, low, high)
        elif low > cur.pos and high >= cur.pos:
            return self._range(cur.right, low, high)    
        else: 
            return  self._range_left(cur.left, low, high) +  [cur.data] + \
                self._range_right(cur.right, low, high)            
            
    
    def _add_all(self, cur):
        if cur is None:
            return []
        
        return self._add_all(cur.left) + [cur.data] +  self._add_all(cur.right)
    
    def _range_right(self, cur, low, high):
        if cur is None:
            return []


        
        # Return a leaf if it's in range
        if cur.left is None and cur.right is None:
            if cur.pos >= low and cur.pos < high:
                return [cur.data]
            else:
                return []
        
        if cur.pos < high:
            return   self._add_all(cur.left) + [cur.data] + self._range_right(cur.right, low, high)
            
        if cur.pos >= high:
            return []
    
    def _range_left(self, cur, low, high):
        if cur is None:
            return []



        
        # Return a leaf if it's in range
        if cur.left is None and cur.right is None:
            if cur.pos >= low and cur.pos < high:
                return [cur.data]
            else:
                return []
        
        if cur.pos >= low:
            return self._range_left(cur.left, low, high) + [cur.data] + self._add_all(cur.right)  

        elif cur.pos < low:
            return []


def test():

    b = RangeBST()
    b.add(4,4)
    b.add(5,5)
    b.add(6,6)
    b.add(4.5, 4.5)
    b.add(1, 1)
    b.add(10,10)
    b.add(2, 2)
    b.add(2.1,2.1)
    b.add(17,17)

    assert(b.range(5,9) == [5,6])
    assert(b.range(1,17) == [1,2,2.1,4,4.5,5,6,10])
    assert(b.range(1,18) == [1,2,2.1,4,4.5,5,6,10,17])
    assert(b.range(4,4.1) == [4])
    assert(b.range(0,1) == [])
    assert(b.range(1,1.4) == [1])
    assert(b.range(1,2) == [1])
    assert(b.range(1,2.001) == [1,2])
    b.remove(4)
    b.remove(5)

    try:
        b.find(5)
        assert(False)
    except:
        pass
    
    assert(b.find(6) == 6)
    assert(b.find(17) == 17)
    assert(b.find(4.5) == 4.5)
    
    
if __name__ == '__main__':
    test()