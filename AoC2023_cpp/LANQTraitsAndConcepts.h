#pragma once
#include <type_traits>

namespace LANQ
{
    namespace ContainerIteratorHelpers
    {
        template<typename Derived>
        class cbegin_cend
        {
        public:
            auto cbegin() const { return This()->begin(); }
            auto cend() const { return This()->end(); }
        private:
            auto This() const { return static_cast<const Derived*>(this); }
        };
    }

    namespace Concepts
    {
        template<typename T>
        struct has_const_iterator
        {
        private:
            template<typename C> static char test(typename C::const_iterator*);
            template<typename C> static int  test(...);
        public:
            enum { value = sizeof(test<T>(0)) == sizeof(char) };
        };


    }
    namespace traits
    {
        namespace details
        {
            // provide 2 overloaded helper functions
            // if the enable_if_t evaluates to true then the
            // overload with int becomes available if not only the overload with double is available

            template<typename type_t>
            static constexpr auto implements_index_helper(int) ->
                std::enable_if_t<std::is_same_v<typename type_t::value_type&, decltype(std::declval<type_t>()[0ul])>, bool>
            {
                return true;
            }

            template<typename type_t>
            static constexpr bool implements_index_helper(double)
            {
                return false;
            }
        }

        template<typename type_t>
        static constexpr bool implements_index()
        {
            // call the helper with an int, if the overload with int is available it will be selected
            // and that returns true otherwise the overload with double (implicit conversion) will be selected
            // and the returns false
            return details::implements_index_helper<type_t>(1);
        }
    }
}