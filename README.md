# Lucky ticket checker

This app can be used to determine whenever current ticket nuber is lucky one.

Ticket is lucky if there is a way to place operator to get expression that equal to  `100`.
For example for ticket with #`123456789` one of possible solution is:

    1+2+3+4+5+6+7+8*9 = 100

Inspired by [this post](https://habrahabr.ru/post/115066/).


In order to build project

    > build.cmd // on windows    
    $ ./build.sh  // on unix   

## Requirements

* .Net 4.6.1
* FSharp 4.0


## Build Status

[![Build status](https://ci.appveyor.com/api/projects/status/1gq6mkaklakw6cth/branch/master?svg=true)](https://ci.appveyor.com/project/ElijahReva/ticket-problem/branch/master)

## Maintainer

- [@ElijahReva](https://github.com/ElijahReva)