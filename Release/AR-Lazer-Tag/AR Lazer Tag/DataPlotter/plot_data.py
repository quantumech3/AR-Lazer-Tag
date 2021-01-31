import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import pandas as pd
from ar_parser import Parser
import time
from time import time as timer

plt.style.use('seaborn')
fig = plt.gcf()

parser = Parser("C:/temp/arLog.csv")

def time_me(f):
    t = time()
    f()
    return timer() - t

def animation(i):
    t = timer()
    artists = parser.update(fig)
    print(timer() - t)

animation = FuncAnimation(fig, func=animation, interval=50)
plt.show()
