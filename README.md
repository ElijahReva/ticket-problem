# Lucky ticket checker
[![.NET Core Desktop](https://github.com/ElijahReva/ticket-problem/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ElijahReva/ticket-problem/actions/workflows/dotnet-desktop.yml)

This app can be used to determine whenever current ticket nuber is lucky one.

Ticket is lucky if there is a way to place operator to get expression that equal to  `100`.
For example for ticket with #`123456789` one of possible solution is:

![Screenshot](/docs/example.jpg)

    1+2+3+4+5+6+7+8*9 = 100

Inspired by [this post](https://habrahabr.ru/post/115066/).

In order to build project

    > build.cmd

## Requirements

* .Net 4.6.1
* FSharp 4.0

## Maintainer

- [@ElijahReva](https://github.com/ElijahReva)
